#!/bin/bash
# Script "User data" pour EC2 (Amazon Linux 2023).
# À coller dans "Détails avancés > Données utilisateur" au lancement de l'instance.
# Il s'exécute une seule fois, en root, au premier démarrage.
# Log d'exécution sur l'instance : /var/log/cloud-init-output.log
set -euxo pipefail

# Image publiée sur Docker Hub (le dépôt doit être public)
IMAGE="asaph01/taskflow:1.0"

dnf install -y docker
systemctl enable --now docker
usermod -aG docker ec2-user   # permet à ec2-user d'utiliser docker sans sudo

docker run -d \
  --name taskflow \
  --restart unless-stopped \
  -p 80:8080 \
  -v taskdata:/app/data \
  "$IMAGE"
