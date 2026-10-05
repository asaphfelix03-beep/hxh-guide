# TaskFlow — Gestionnaire de tâches conteneurisé et déployé sur AWS

Application web en **C# / ASP.NET Core (.NET 10)**, conteneurisée avec **Docker** et déployée sur une instance **AWS EC2**, accessible publiquement sur Internet.

| | Lien |
|---|---|
| Code source | https://github.com/asaphfelix03-beep/taskflow |
| Image Docker | https://hub.docker.com/r/asaph01/taskflow |
| Application en ligne | http://&lt;IP-PUBLIQUE-EC2&gt; |

## 1. Présentation

**Fonctionnalités**
- Créer, modifier, supprimer des tâches (titre, description, priorité, échéance)
- Marquer une tâche comme terminée, filtrer : toutes / en cours / terminées
- Échéances dépassées mises en évidence
- API REST JSON sur `/api/tasks`
- Endpoint de supervision `/health` (état de la base, version, nom du conteneur)

**Technologies**

| Couche | Choix |
|---|---|
| Langage / framework | C# 14, ASP.NET Core 10 (Razor Pages + Minimal API) |
| Base de données | SQLite via Entity Framework Core 10 |
| Interface | Bootstrap 5 + Bootstrap Icons (CDN) |
| Conteneur | Docker, build multi-étapes (`sdk:10.0` → `aspnet:10.0`), utilisateur non-root |
| Registre d'images | Docker Hub |
| Hébergement | AWS EC2 `t3.micro`, Amazon Linux 2023 |

**Architecture**

```
 Utilisateur (navigateur)
        │  HTTP :80
        ▼
 ┌──────────────────────── AWS ────────────────────────┐
 │  Security Group : 80 ouvert à tous, 22 = mon IP     │
 │  ┌──────────── Instance EC2 (t3.micro) ───────────┐ │
 │  │  Docker Engine                                 │ │
 │  │   └─ conteneur "taskflow"  (port 80 → 8080)    │ │
 │  │        └─ ASP.NET Core (Kestrel)               │ │
 │  │             └─ /app/data/tasks.db ◄── volume   │ │
 │  │                                     "taskdata" │ │
 │  └────────────────────────────────────────────────┘ │
 └─────────────────────────────────────────────────────┘
        ▲
        │ docker pull
 Docker Hub : asaph01/taskflow:1.0
```

**Arborescence**

```
TaskFlow/
├── Program.cs              # Démarrage : base, pages, API, /health
├── Models/                 # TaskItem (entité), TaskInput (saisie validée), Priority
├── Persistence/            # DbContext EF Core, requêtes, données d'exemple
├── Endpoints/TaskApi.cs    # API REST /api/tasks + /health
├── Pages/                  # Interface Razor Pages (liste, création, modification)
├── wwwroot/css/site.css
├── Dockerfile              # Image multi-étapes
├── docker-compose.yml      # Test en local
└── deploy/
    ├── aws-deploy.sh       # Création de l'infrastructure AWS en une commande (AWS CLI)
    ├── aws-destroy.sh      # Suppression de toutes les ressources AWS
    ├── user-data.sh        # Installation automatique de Docker + lancement du conteneur sur EC2
    └── update.sh           # Mise à jour de la version déployée
```

## 2. Lancer en local

**Prérequis :** Docker Desktop (le SDK .NET n'est pas nécessaire, la compilation se fait dans Docker).

```bash
docker compose up --build
```

Ouvrir http://localhost:8081. Arrêter avec `Ctrl+C` puis `docker compose down`.

> Avec le SDK .NET 10 installé, on peut aussi lancer `dotnet run` → http://localhost:5080.

## 3. Publier l'image sur Docker Hub

1. Créer un compte sur https://hub.docker.com (gratuit).
2. Dans le dossier du projet :

```bash
docker login
docker build -t asaph01/taskflow:1.0 .
docker push asaph01/taskflow:1.0
```

3. Vérifier sur Docker Hub que le dépôt `taskflow` est **public** (sinon l'instance EC2 ne pourra pas le télécharger sans identifiants).

> ⚠️ Un PC Windows/Intel produit une image `linux/amd64`, compatible avec `t3.micro`.
> Sur un Mac Apple Silicon, ajouter `--platform linux/amd64` au `docker build`.

## 4. Créer l'instance EC2

### Option A — En une commande avec AWS CLI (méthode utilisée)

Prérequis : [AWS CLI v2](https://aws.amazon.com/cli/) configuré avec `aws configure`
(ou, pour AWS Academy / Learner Lab : copier le bloc *AWS CLI* de « AWS Details » dans `~/.aws/credentials`, région `us-east-1`).

Depuis Git Bash, à la racine du projet :

```bash
./deploy/aws-deploy.sh
```

Le script :
1. crée la paire de clés `taskflow-key` (clé privée enregistrée dans `taskflow-key.pem`, ignorée par git) ;
2. crée le Security Group `taskflow-sg` : HTTP 80 ouvert à tous, SSH 22 limité à ton IP ;
3. récupère la dernière AMI Amazon Linux 2023 (paramètre public SSM) ;
4. lance une instance `t3.micro` nommée `taskflow-server` avec `deploy/user-data.sh` en données utilisateur ;
5. attend que `http://<IP>/health` réponde et affiche l'URL publique.

Le script peut être relancé sans risque : il réutilise ce qui existe déjà.

### Option B — Dans la console AWS

Console AWS → **EC2** → **Lancer une instance** :

| Paramètre | Valeur |
|---|---|
| Nom | `taskflow-server` |
| AMI | **Amazon Linux 2023** (64 bits x86) |
| Type d'instance | **t3.micro** (ou t2.micro, éligibles à l'offre gratuite) |
| Paire de clés | Créer une paire `taskflow-key`, type RSA, format `.pem` → le fichier se télécharge |
| Réseau | Attribuer automatiquement une IP publique : **Activé** |
| Security Group | Créer : **SSH (22)** source *Mon IP* + **HTTP (80)** source *0.0.0.0/0* (n'importe où) |
| Stockage | 8 Go gp3 (par défaut) |
| Détails avancés → **Données utilisateur** | Coller le contenu de `deploy/user-data.sh` |

Cliquer **Lancer l'instance**, attendre que l'état soit *En cours d'exécution* et que les vérifications soient OK (2–3 min). Le script installe Docker et démarre le conteneur tout seul.

## 5. Vérifier le déploiement

Copier l'**adresse IPv4 publique** de l'instance, puis ouvrir dans un navigateur :

- `http://<IP-PUBLIQUE>` → l'application
- `http://<IP-PUBLIQUE>/health` → `{"status":"healthy", ...}`
- `http://<IP-PUBLIQUE>/api/tasks` → la liste des tâches en JSON

> Utiliser `http://` et non `https://` : l'application est servie en HTTP sur le port 80.

### Se connecter en SSH (pour les captures `docker ps`, les logs…)

Depuis PowerShell, dans le dossier où se trouve `taskflow-key.pem` :

```powershell
# Une seule fois : restreindre les droits du fichier, sinon SSH refuse la clé
icacls .\taskflow-key.pem /inheritance:r
icacls .\taskflow-key.pem /grant:r "$($env:USERNAME):(R)"

ssh -i .\taskflow-key.pem ec2-user@<IP-PUBLIQUE>
```

Sur l'instance :

```bash
docker ps                                 # le conteneur taskflow doit être "Up"
docker logs taskflow                      # logs de l'application
docker volume ls                          # le volume taskdata
curl -s localhost/health                  # test local depuis l'instance
sudo cat /var/log/cloud-init-output.log   # log du script User data
```

### Alternative sans User data (installation manuelle)

Si l'instance a été lancée sans le script, en SSH :

```bash
sudo dnf install -y docker
sudo systemctl enable --now docker
sudo docker run -d --name taskflow --restart unless-stopped \
  -p 80:8080 -v taskdata:/app/data asaph01/taskflow:1.0
```

## 6. Tester l'API

```bash
# Lister
curl http://<IP-PUBLIQUE>/api/tasks
curl "http://<IP-PUBLIQUE>/api/tasks?status=active"

# Créer
curl -X POST http://<IP-PUBLIQUE>/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Tester l API","priority":"High","dueDate":"2026-12-01"}'

# Modifier (id 4)
curl -X PUT http://<IP-PUBLIQUE>/api/tasks/4 \
  -H "Content-Type: application/json" \
  -d '{"title":"Tester l API","priority":"Normal","isDone":true}'

# Supprimer
curl -X DELETE http://<IP-PUBLIQUE>/api/tasks/4
```

Valeurs de `priority` : `Low`, `Normal`, `High`. Format de `dueDate` : `AAAA-MM-JJ`.

## 7. Mettre à jour l'application

1. Modifier le code, changer `<Version>` dans `TaskFlow.csproj` (ex. `1.1.0`).
2. Sur le PC :
   ```bash
   docker build -t asaph01/taskflow:1.1 .
   docker push asaph01/taskflow:1.1
   ```
3. Sur l'instance (copier `deploy/update.sh` ou le recréer avec `nano update.sh`) :
   ```bash
   chmod +x update.sh
   ./update.sh asaph01/taskflow:1.1
   ```

Les tâches sont conservées : elles sont stockées dans le volume `taskdata`, pas dans le conteneur. La nouvelle version s'affiche en bas de page.

## 8. Captures d'écran à fournir

1. Docker Hub : le dépôt `taskflow` avec le tag `1.0`
2. Console EC2 : l'instance *En cours d'exécution* avec son IP publique
3. Security Group : règles entrantes 22 et 80
4. Terminal SSH : `docker ps` et `docker images`
5. Navigateur : `http://<IP-PUBLIQUE>` avec l'application (le pied de page affiche le nom du conteneur)
6. Navigateur : `http://<IP-PUBLIQUE>/health`

## 9. Dépannage

| Problème | Cause probable / solution |
|---|---|
| La page ne charge pas (timeout) | Port 80 absent du Security Group, ou `https://` utilisé au lieu de `http://` |
| `docker ps` vide | Voir `sudo cat /var/log/cloud-init-output.log` (nom d'image erroné ? dépôt privé ?) |
| `permission denied ... docker.sock` | Se déconnecter/reconnecter en SSH (groupe docker), ou préfixer avec `sudo` |
| `exec format error` dans les logs | Image construite pour ARM : rebuild avec `--platform linux/amd64` |
| `UNPROTECTED PRIVATE KEY FILE` | Lancer les commandes `icacls` de la section 5 |
| L'IP change après un arrêt/redémarrage | Normal ; associer une **Elastic IP** pour une adresse fixe |

## 10. Nettoyage (éviter les frais)

Après la notation, supprimer l'instance, le Security Group et la paire de clés :

```bash
./deploy/aws-destroy.sh
```

Ou dans la console : EC2 → Instances → sélectionner l'instance → **État de l'instance → Résilier**.
Supprimer aussi l'Elastic IP si une a été créée (une IP réservée non utilisée est facturée).
