# Le Guide H×H — guide du manga Hunter × Hunter, conteneurisé et déployé sur AWS

Application web en **C# / ASP.NET Core (.NET 10)** : le guide du manga *Hunter × Hunter* (arcs, personnages, Nen)
et, pour chaque lecteur, un **suivi de lecture tome par tome sans spoiler**. Elle tourne dans **3 conteneurs Docker**
(Caddy, application, PostgreSQL) sur une instance **AWS EC2**, en **HTTPS**.

| | Lien |
|---|---|
| Application en ligne | https://13-48-46-246.sslip.io |
| Code source | https://github.com/asaphfelix03-beep/hxh-guide |
| Image Docker | https://hub.docker.com/r/asaph01/hxh-guide |
| Compte de démonstration | `demo@guide.local` / `Demo1234` (tomes 1 à 24 lus) |

> Guide de fan non officiel, réalisé dans un cadre étudiant. *Hunter × Hunter* est un manga de Yoshihiro Togashi.
> Aucune image ni page du manga n'est reproduite : tous les textes sont originaux et tous les visuels sont dessinés en CSS/SVG.

## 1. Fonctionnalités

- **Les 8 arcs** : frise proportionnelle au nombre de tomes, fiche de chaque arc (tomes, chapitres, résumé), note moyenne et avis des lecteurs
- **21 personnages** : type de Nen, affiliation, capacité emblématique, arc d'apparition ; filtres par Nen, par arc et par favoris
- **Mon classeur** : les 38 tomes comme les emplacements de cartes du classeur de Greed Island ; un clic coche un tome lu,
  ou « j'ai tout lu jusqu'au tome N » ; progression globale et par arc
- **Mode sans spoiler** : résumé, avis et nouveaux personnages des arcs pas encore atteints sont masqués (affichables d'un clic) ; désactivable
- **Avis** : une note de 1 à 5 étoiles et un commentaire par arc et par lecteur, modifiable
- **Favoris** et page **Communauté** : arcs les mieux notés, personnages les plus aimés, derniers avis
- **Guide du Nen** : les six types, l'hexagone des affinités, la divination par l'eau
- **Comptes** (inscription, connexion), mode sombre, interface responsive
- **API JSON** : `/api/arcs` (publique), `/api/moi` (progression du lecteur connecté), `/health` (supervision)

## 2. Architecture

```
            Internet
               │  HTTPS :443 (HTTP :80 → redirection)
               ▼
┌──────────────────────── AWS eu-north-1 ─────────────────────────┐
│ Security Group : 80/443 seulement (pas de SSH)                 │
│ ┌──────────────── Instance EC2 t3.micro (AL2023) ─────────────┐ │
│ │  Docker Compose — réseau privé                              │ │
│ │                                                             │ │
│ │   caddy ──────────► app ─────────────► db                   │ │
│ │   reverse proxy     ASP.NET Core 10    PostgreSQL 17        │ │
│ │   certificat TLS    port 8080          port 5432            │ │
│ │   Let's Encrypt     (non exposé)       (non exposé)         │ │
│ │       │                                   │                 │ │
│ │   volume caddy_data                  volume pgdata          │ │
│ └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                           ▲ docker pull
              Docker Hub : asaph01/hxh-guide:1.1
```

- Seul **Caddy** est exposé. L'application et la base ne sont joignables que sur le réseau Docker interne.
- **sslip.io** transforme l'IP en nom de domaine (`13-48-46-246.sslip.io`) : vrai certificat HTTPS gratuit sans acheter de domaine.
- Le **contenu du manga** (arcs, personnages) est dans le code (`Models/Catalog.cs`) ; la **base** ne stocke que les données des lecteurs
  (tomes lus, avis, favoris). Schéma géré par **migrations EF Core**, appliquées au démarrage.

| Couche | Choix |
|---|---|
| Application | C# 14, ASP.NET Core 10 Razor Pages + Minimal API |
| Authentification | ASP.NET Core Identity (cookies, mots de passe hachés) |
| Base de données | PostgreSQL 17 via Entity Framework Core 10 (Npgsql) |
| Interface | Bootstrap 5.3 servi localement, police Dela Gothic One auto-hébergée, icônes SVG en sprite |
| Reverse proxy / HTTPS | Caddy 2 (Let's Encrypt automatique, HTTP/2 et HTTP/3, compression) |
| Conteneurs | Dockerfile multi-étapes (ReadyToRun, utilisateur non-root), Docker Compose |
| Cloud | AWS EC2 t3.micro, Amazon Linux 2023, déploiement scripté avec AWS CLI |

```
├── Program.cs                  # Démarrage : PostgreSQL, Identity, pages, API, migrations
├── Models/Catalog.cs           # Contenu du guide : 8 arcs, 38 tomes, 21 personnages
├── Models/                     # Lecteur, tomes lus, avis, favoris, types de Nen
├── Persistence/                # DbContext, migrations, état de lecture (anti-spoiler), démo
├── Pages/                      # Arcs/, Personnages/, Classeur, Communaute, Nen/, Compte/
├── Endpoints/Api.cs            # API JSON + /health
├── Dockerfile, docker-compose.yml
└── deploy/                     # Compose de production, Caddyfile, scripts EC2 et AWS
```

## 3. Lancer en local

Prérequis : Docker Desktop.

```bash
docker compose up --build
```

Ouvrir http://localhost:8081 (compte `demo@guide.local` / `Demo1234`).

## 4. Publier l'image

```bash
docker build -t asaph01/hxh-guide:1.0 .
docker push asaph01/hxh-guide:1.0
```

## 5. Déployer sur AWS

Prérequis : AWS CLI v2 connecté (`aws login`). Depuis Git Bash, à la racine du projet :

```bash
./deploy/aws-deploy.sh
```

Le script crée un rôle IAM minimal pour Systems Manager, le Security Group `hxh-sg` (80/443 uniquement),
puis lance une instance `t3.micro` (disque chiffré, IMDSv2 obligatoire) avec `deploy/user-data.sh`,
et attend que `https://<ip>.sslip.io/health` réponde. `user-data.sh` installe Docker et Docker Compose,
récupère la configuration depuis ce dépôt, génère le fichier `.env` (domaine, **mot de passe PostgreSQL aléatoire**)
et démarre les conteneurs dans `/opt/hxh-guide`.

**Administration sans SSH**, par AWS Systems Manager :

```bash
aws ssm start-session --target <id-instance>          # terminal sur le serveur (plugin Session Manager)
aws ssm send-command --instance-ids <id-instance> --document-name AWS-RunShellScript \
  --parameters 'commands=["cd /opt/hxh-guide && docker compose ps"]'
```

Mettre à jour : `docker build` + `docker push` d'un nouveau tag, puis sur le serveur `/opt/hxh-guide/update.sh 1.2`.

## 6. Sécurité

| Niveau | Mesure |
|---|---|
| Réseau AWS | Security Group : seuls 80 (redirection) et 443 (HTTPS, HTTP/3) sont ouverts. **Aucun port SSH**, service SSH désactivé |
| Administration | AWS Systems Manager : accès authentifié par IAM et journalisé, sans clé SSH à protéger |
| Instance | IMDSv2 obligatoire (protège les identifiants du rôle), rôle IAM minimal (`AmazonSSMManagedInstanceCore`), mises à jour de sécurité appliquées |
| Transport | HTTPS Let's Encrypt, redirection HTTP → HTTPS, HSTS, TLS 1.2 minimum |
| Conteneurs | Application non-root, système de fichiers en lecture seule, aucune capacité Linux, `no-new-privileges`, mémoire et journaux limités |
| Réseau Docker | Base PostgreSQL sur un réseau interne sans accès Internet, injoignable depuis Caddy ; aucun port exposé hors 80/443 |
| Secrets | Mot de passe PostgreSQL aléatoire généré sur le serveur (`.env`, lisible par root uniquement), jamais dans Git |
| Application | CSP stricte (aucun script en ligne), `X-Frame-Options`, `Permissions-Policy`, en-tête `Server` masqué |
| Comptes | Mots de passe hachés (PBKDF2), compte bloqué après 5 échecs, **10 tentatives/minute/IP** sur connexion et inscription |
| Formulaires | Jeton anti-CSRF sur tous les envois, validation côté serveur, encodage HTML de tous les textes saisis (anti-XSS) |
| Données | Requêtes paramétrées (EF Core, anti-injection SQL), redirections de connexion limitées au site (anti-redirection ouverte) |

Limites connues : le compte de démonstration est public (n'importe qui peut y laisser des avis) ;
l'inscription indique si une adresse est déjà utilisée.

## 7. Captures d'écran pour le rapport

1. Docker Hub : `asaph01/hxh-guide` (tags 1.0 et 1.1)
2. Console EC2 : l'instance, le Security Group (80/443 seulement), le rôle IAM
3. Systems Manager → Fleet Manager : l'instance « Online » ; Run Command : `docker compose ps`
4. Navigateur : le cadenas HTTPS, l'accueil, le classeur, une fiche d'arc masquée par le mode sans spoiler
5. Outils de développement → Réseau : les en-têtes de sécurité (CSP, HSTS)

## 8. Nettoyage

```bash
./deploy/aws-destroy.sh
```
