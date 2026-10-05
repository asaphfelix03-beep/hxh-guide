#!/bin/bash
# Script "User data" pour EC2 (Amazon Linux 2023), exécuté une seule fois, en root, au premier démarrage.
# Installe Docker + Docker Compose, récupère la configuration depuis GitHub et démarre les 3 conteneurs
# (Caddy HTTPS, application, PostgreSQL). Log : /var/log/cloud-init-output.log
set -euxo pipefail

REPO_RAW="https://raw.githubusercontent.com/asaphfelix03-beep/taskflow/main/deploy"
APP_DIR=/opt/taskflow

dnf install -y docker
systemctl enable --now docker
usermod -aG docker ec2-user   # permet à ec2-user d'utiliser docker sans sudo

# Plugin Docker Compose (absent des dépôts Amazon Linux)
mkdir -p /usr/local/lib/docker/cli-plugins
curl -fsSL "https://github.com/docker/compose/releases/latest/download/docker-compose-linux-$(uname -m)" \
  -o /usr/local/lib/docker/cli-plugins/docker-compose
chmod +x /usr/local/lib/docker/cli-plugins/docker-compose

mkdir -p "$APP_DIR"
cd "$APP_DIR"
curl -fsSL "$REPO_RAW/docker-compose.prod.yml" -o docker-compose.yml
curl -fsSL "$REPO_RAW/Caddyfile" -o Caddyfile
curl -fsSL "$REPO_RAW/update.sh" -o update.sh
chmod +x update.sh

# Nom de domaine sslip.io dérivé de l'IP publique (métadonnées EC2, IMDSv2) : 13.48.46.246 -> 13-48-46-246.sslip.io
TOKEN=$(curl -fsS -X PUT http://169.254.169.254/latest/api/token -H "X-aws-ec2-metadata-token-ttl-seconds: 300")
PUBLIC_IP=$(curl -fsS -H "X-aws-ec2-metadata-token: $TOKEN" http://169.254.169.254/latest/meta-data/public-ipv4)

# Secrets générés sur le serveur, jamais dans le dépôt. Lisible par root uniquement.
if [ ! -f .env ]; then
  cat > .env <<EOF
DOMAIN=${PUBLIC_IP//./-}.sslip.io
IMAGE=asaph01/taskflow:2.0
POSTGRES_PASSWORD=$(openssl rand -hex 24)
SEED_DEMO=true
EOF
  chmod 600 .env
fi

docker compose up -d
