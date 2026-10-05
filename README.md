# TaskFlow — Gestion de projets en équipe, conteneurisée et déployée sur AWS

Application web collaborative en **C# / ASP.NET Core (.NET 10)** : comptes utilisateurs, projets partagés,
tableau Kanban avec glisser-déposer. Elle tourne dans **3 conteneurs Docker** (Caddy, application, PostgreSQL)
sur une instance **AWS EC2**, en **HTTPS**.

| | Lien |
|---|---|
| Application en ligne | https://13-48-46-246.sslip.io |
| Code source | https://github.com/asaphfelix03-beep/taskflow |
| Image Docker | https://hub.docker.com/r/asaph01/taskflow |
| Compte de démonstration | `demo@taskflow.local` / `Demo1234` |

## 1. Fonctionnalités

- **Comptes** : inscription, connexion (mot de passe haché par ASP.NET Core Identity, blocage après 5 échecs), déconnexion
- **Projets d'équipe** : création, membres invités par e-mail, rôles propriétaire / membre, quitter ou supprimer un projet
- **Tableau Kanban** : colonnes À faire / En cours / Terminé, **glisser-déposer** enregistré en base, ajout rapide, filtre instantané
- **Tâches** : description, priorité, échéance (retards en rouge), **assignation** à un membre, **étiquettes** colorées
- **Tableau de bord** : mes tâches ouvertes, en retard, à rendre sous 7 jours, terminées cette semaine, avancement des projets
- **Recherche** dans tous ses projets (titre, description, #étiquette), filtres par colonne et « assignées à moi »
- **Mode sombre**, interface responsive (mobile : colonnes défilables)
- **API JSON** (`/api/projects`, `/api/projects/{id}/tasks`) réservée aux utilisateurs connectés
- **Supervision** : `/health` (état de PostgreSQL, version, nom du conteneur)

## 2. Architecture

```
            Internet
               │  HTTPS :443 (HTTP :80 → redirection)
               ▼
┌──────────────────────── AWS eu-north-1 ─────────────────────────┐
│ Security Group : 80/443 ouverts, 22 limité à mon IP             │
│ ┌──────────────── Instance EC2 t3.micro (AL2023) ─────────────┐ │
│ │  Docker Compose — réseau privé                              │ │
│ │                                                             │ │
│ │   caddy ──────────► app ─────────────► db                   │ │
│ │   reverse proxy     ASP.NET Core 10    PostgreSQL 17        │ │
│ │   certificat TLS    port 8080          port 5432            │ │
│ │   Let's Encrypt     (non exposé)       (non exposé)         │ │
│ │   compression          │                  │                 │ │
│ │       │                │                  ▼                 │ │
│ │   volume caddy_data    │             volume pgdata          │ │
│ └────────────────────────┼────────────────────────────────────┘ │
└──────────────────────────┼──────────────────────────────────────┘
                           │ docker pull
              Docker Hub : asaph01/taskflow:2.0
```

- Seul **Caddy** est exposé à Internet. L'application et la base ne sont joignables que sur le réseau Docker interne.
- **sslip.io** transforme l'IP en nom de domaine (`13-48-46-246.sslip.io`), ce qui permet un vrai certificat HTTPS gratuit sans acheter de domaine.
- Le **schéma de la base** est géré par des **migrations EF Core**, appliquées automatiquement au démarrage de l'application.
- Les **clés de chiffrement des cookies** sont stockées en base : remplacer le conteneur ne déconnecte personne.

**Technologies**

| Couche | Choix |
|---|---|
| Application | C# 14, ASP.NET Core 10 Razor Pages + Minimal API |
| Authentification | ASP.NET Core Identity (cookies, mots de passe hachés PBKDF2) |
| Base de données | PostgreSQL 17 via Entity Framework Core 10 (Npgsql), migrations |
| Interface | Bootstrap 5.3 servi localement, icônes SVG en sprite, JavaScript natif (glisser-déposer) |
| Reverse proxy / HTTPS | Caddy 2 (Let's Encrypt automatique, HTTP/2 et HTTP/3, compression zstd/gzip) |
| Conteneurs | Dockerfile multi-étapes (ReadyToRun, utilisateur non-root), Docker Compose |
| Cloud | AWS EC2 t3.micro, Amazon Linux 2023, déploiement scripté avec AWS CLI |

**Arborescence**

```
TaskFlow/
├── Program.cs                  # Démarrage : PostgreSQL, Identity, pages, API, migrations
├── Models/                     # Project, ProjectMember, TaskItem, AppUser, saisies validées
├── Persistence/                # DbContext, migrations, règles d'accès, données de démo
├── Endpoints/Api.cs            # API JSON + /health
├── Pages/                      # Razor Pages : Account/, Projects/ (Board, Settings), Tasks/, Search
├── TagHelpers.cs               # <icon>, <avatar>, <tag-chip>
├── wwwroot/                    # CSS, JS (board.js : glisser-déposer), Bootstrap local
├── Dockerfile
├── docker-compose.yml          # Développement local : app + PostgreSQL
└── deploy/
    ├── docker-compose.prod.yml # Production : Caddy + app + PostgreSQL
    ├── Caddyfile               # HTTPS, en-têtes de sécurité, reverse proxy
    ├── .env.example            # Variables (domaine, image, mot de passe PostgreSQL)
    ├── user-data.sh            # Installation automatique au premier démarrage EC2
    ├── update.sh               # Mise à jour sans perte de données (sur le serveur)
    ├── aws-deploy.sh           # Création de l'infrastructure AWS en une commande
    └── aws-destroy.sh          # Suppression de toutes les ressources AWS
```

## 3. Lancer en local

Prérequis : Docker Desktop (le SDK .NET n'est pas nécessaire).

```bash
docker compose up --build
```

Ouvrir http://localhost:8081 et se connecter avec `demo@taskflow.local` / `Demo1234`.
Arrêter avec `docker compose down` (ajouter `-v` pour effacer aussi la base).

## 4. Publier l'image

```bash
docker build -t asaph01/taskflow:2.0 .
docker push asaph01/taskflow:2.0
```

## 5. Déployer sur AWS

### En une commande (AWS CLI)

Prérequis : AWS CLI v2 connecté (`aws login`). Depuis Git Bash, à la racine du projet :

```bash
./deploy/aws-deploy.sh
```

Le script crée la paire de clés `taskflow-key`, le Security Group `taskflow-sg` (80/443 publics, 22 limité à ton IP),
lance une instance `t3.micro` Amazon Linux 2023 avec `deploy/user-data.sh`, puis attend que `https://<ip>.sslip.io/health` réponde.
Il peut être relancé sans risque : il réutilise ce qui existe déjà.

`user-data.sh` installe Docker et Docker Compose, télécharge `docker-compose.prod.yml` et `Caddyfile` depuis ce dépôt,
génère le fichier `.env` (domaine sslip.io, **mot de passe PostgreSQL aléatoire**) et démarre les conteneurs.

**Sans rien installer** : depuis AWS CloudShell (icône `>_` de la console) :

```bash
git clone https://github.com/asaphfelix03-beep/taskflow.git && cd taskflow && MY_IP=<IP-de-ton-PC> ./deploy/aws-deploy.sh
```

### Vérifier

```bash
ssh -i taskflow-key.pem ec2-user@<IP>
cd /opt/taskflow
sudo docker compose ps            # caddy, app, db : Up (db healthy)
sudo docker compose logs -f app   # logs de l'application
sudo docker compose logs caddy    # obtention du certificat HTTPS
```

## 6. Mettre à jour

1. Changer `<Version>` dans `TaskFlow.csproj`, puis `docker build -t asaph01/taskflow:2.1 .` et `docker push`.
2. Sur le serveur : `sudo /opt/taskflow/update.sh 2.1`

Les données restent dans le volume `pgdata` ; les nouvelles migrations s'appliquent au démarrage.

## 7. Sécurité

- **HTTPS** partout, redirection automatique, HSTS et en-têtes de sécurité (Caddyfile)
- Mots de passe **hachés** (Identity), blocage du compte après 5 échecs, cookies `HttpOnly` et `SameSite`
- Protection **CSRF** sur tous les formulaires et sur le glisser-déposer (jeton anti-falsification)
- **Contrôle d'accès** centralisé (`Persistence/AccessQueries.cs`) : un utilisateur ne voit que les projets dont il est membre ;
  les autres projets renvoient 404
- Conteneur applicatif **non-root** ; PostgreSQL non exposé à Internet ; mot de passe généré sur le serveur, jamais dans Git
- SSH limité à une seule adresse IP

## 8. Captures d'écran pour le rapport

1. Docker Hub : le dépôt `asaph01/taskflow` avec les tags `1.0` et `2.0`
2. Console EC2 : l'instance, son IP publique, les règles du Security Group (22, 80, 443)
3. SSH : `sudo docker compose ps` (3 conteneurs) et `docker images`
4. Navigateur : le **cadenas HTTPS**, le tableau Kanban, le tableau de bord
5. `https://13-48-46-246.sslip.io/health`

## 9. Dépannage

| Problème | Cause probable / solution |
|---|---|
| Erreur de certificat HTTPS au premier lancement | Caddy obtient le certificat en ~30 s ; vérifier `docker compose logs caddy` et que le port 80 est ouvert |
| `/health` renvoie `unhealthy` | PostgreSQL ne répond pas : `docker compose ps db`, `docker compose logs db` |
| Page blanche / 502 | L'application démarre encore ou a planté : `docker compose logs app` |
| `permission denied ... docker.sock` | Préfixer avec `sudo`, ou se reconnecter en SSH (groupe docker) |
| L'IP change après arrêt/redémarrage de l'instance | Le domaine sslip.io change aussi : associer une **Elastic IP** et mettre à jour `DOMAIN` dans `/opt/taskflow/.env` |

## 10. Nettoyage (éviter les frais)

```bash
./deploy/aws-destroy.sh
```

Supprime l'instance, le Security Group et la paire de clés.
