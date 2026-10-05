#!/bin/bash
# Met à jour le Guide H×H sur le serveur, sans perte de données.
# Usage (sur l'instance) : sudo /opt/hxh-guide/update.sh 2.1
# Les données restent dans le volume PostgreSQL ; les migrations de schéma s'appliquent au démarrage de l'app.
set -euo pipefail

cd /opt/hxh-guide
if [ -n "${1:-}" ]; then
  sed -i "s#^IMAGE=.*#IMAGE=asaph01/hxh-guide:$1#" .env
fi

docker compose pull app
docker compose up -d
docker compose ps
