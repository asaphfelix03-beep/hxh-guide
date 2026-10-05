#!/bin/bash
# Met à jour l'application sur l'instance EC2 avec une nouvelle version de l'image.
# Usage : ./update.sh asaph01/taskflow:1.1
# Les tâches sont conservées : elles sont dans le volume "taskdata", pas dans le conteneur.
set -euo pipefail

IMAGE="${1:?Usage : $0 <image:tag>}"

docker pull "$IMAGE"
docker rm -f taskflow 2>/dev/null || true
docker run -d \
  --name taskflow \
  --restart unless-stopped \
  -p 80:8080 \
  -v taskdata:/app/data \
  "$IMAGE"

docker ps --filter name=taskflow
