#!/bin/bash
# Déploie le Guide H×H sur une instance EC2 avec AWS CLI v2.
# Crée (ou réutilise) : une paire de clés SSH, un Security Group et une instance t3.micro
# Amazon Linux 2023 qui exécute deploy/user-data.sh au premier démarrage.
#
# Prérequis : AWS CLI configuré (aws configure) et image publiée sur Docker Hub.
# Usage (Git Bash, Linux ou macOS) : ./deploy/aws-deploy.sh
set -euo pipefail
export MSYS_NO_PATHCONV=1   # Git Bash sous Windows : ne pas convertir les arguments "/aws/...", "/inheritance:r"

cd "$(dirname "$0")/.."

NAME="hxh"
KEY_NAME="$NAME-key"
SG_NAME="$NAME-sg"
INSTANCE_NAME="$NAME-server"
INSTANCE_TYPE="${INSTANCE_TYPE:-t3.micro}"
REGION="$(aws configure get region || true)"
export AWS_DEFAULT_REGION="${REGION:-us-east-1}"

echo "==> Compte AWS $(aws sts get-caller-identity --query Account --output text), région $AWS_DEFAULT_REGION"

# 1. Paire de clés SSH (la clé privée n'est téléchargeable qu'à la création)
if aws ec2 describe-key-pairs --key-names "$KEY_NAME" >/dev/null 2>&1; then
  echo "==> Paire de clés $KEY_NAME déjà existante"
else
  aws ec2 create-key-pair --key-name "$KEY_NAME" --key-type rsa --key-format pem \
    --query KeyMaterial --output text | tr -d '\r' > "$KEY_NAME.pem"
  if command -v icacls.exe >/dev/null; then
    icacls.exe "$KEY_NAME.pem" /inheritance:r /grant:r "$USERNAME:(R)" >/dev/null
  else
    chmod 400 "$KEY_NAME.pem"
  fi
  echo "==> Clé privée enregistrée dans $KEY_NAME.pem (ne jamais la publier)"
fi

# 2. Security Group : HTTP (80) ouvert à tous, SSH (22) limité à ton adresse IP
VPC_ID=$(aws ec2 describe-vpcs --filters Name=is-default,Values=true --query 'Vpcs[0].VpcId' --output text)
if [ "$VPC_ID" = "None" ]; then
  echo "Aucun VPC par défaut dans la région $AWS_DEFAULT_REGION." >&2
  exit 1
fi

SG_ID=$(aws ec2 describe-security-groups \
  --filters Name=group-name,Values="$SG_NAME" Name=vpc-id,Values="$VPC_ID" \
  --query 'SecurityGroups[0].GroupId' --output text)
if [ "$SG_ID" = "None" ]; then
  SG_ID=$(aws ec2 create-security-group --group-name "$SG_NAME" --vpc-id "$VPC_ID" \
    --description "Guide HxH - web public, SSH restreint" --query GroupId --output text)
  # MY_IP peut être fourni (ex. depuis CloudShell, pour autoriser l'IP de ton PC et non celle de CloudShell)
  MY_IP="${MY_IP:-$(curl -fs https://checkip.amazonaws.com | tr -d '[:space:]')}"
  aws ec2 authorize-security-group-ingress --group-id "$SG_ID" --protocol tcp --port 22 --cidr "$MY_IP/32" >/dev/null
  echo "==> Security Group $SG_ID créé (SSH réservé à $MY_IP)"
else
  echo "==> Security Group $SG_ID déjà existant"
fi

# Ports web ouverts à tous : 80 (redirection + validation Let's Encrypt), 443 TCP (HTTPS) et 443 UDP (HTTP/3).
# Une règle déjà présente renvoie "Duplicate" : on l'ignore, le script peut être relancé.
for rule in tcp:80 tcp:443 udp:443; do
  aws ec2 authorize-security-group-ingress --group-id "$SG_ID" \
    --protocol "${rule%%:*}" --port "${rule##*:}" --cidr 0.0.0.0/0 >/dev/null 2>&1 || true
done

# 3. Instance EC2 (réutilisée si elle tourne déjà)
INSTANCE_ID=$(aws ec2 describe-instances \
  --filters Name=tag:Name,Values="$INSTANCE_NAME" Name=instance-state-name,Values=pending,running \
  --query 'Reservations[0].Instances[0].InstanceId' --output text)
if [ "$INSTANCE_ID" = "None" ]; then
  AMI_ID=$(aws ssm get-parameters \
    --names /aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64 \
    --query 'Parameters[0].Value' --output text)

  # Fins de ligne Linux obligatoires : un script en CRLF (édité sous Windows) ferait échouer bash sur l'instance.
  USER_DATA="deploy/.user-data.lf"
  tr -d '\r' < deploy/user-data.sh > "$USER_DATA"
  trap 'rm -f "$USER_DATA"' EXIT

  INSTANCE_ID=$(aws ec2 run-instances \
    --image-id "$AMI_ID" \
    --instance-type "$INSTANCE_TYPE" \
    --key-name "$KEY_NAME" \
    --security-group-ids "$SG_ID" \
    --user-data "file://$USER_DATA" \
    --tag-specifications "ResourceType=instance,Tags=[{Key=Name,Value=$INSTANCE_NAME}]" \
    --query 'Instances[0].InstanceId' --output text)
  echo "==> Instance $INSTANCE_ID lancée ($INSTANCE_TYPE, AMI $AMI_ID)"
else
  echo "==> Instance $INSTANCE_ID déjà en cours d'exécution"
fi

aws ec2 wait instance-running --instance-ids "$INSTANCE_ID"
PUBLIC_IP=$(aws ec2 describe-instances --instance-ids "$INSTANCE_ID" \
  --query 'Reservations[0].Instances[0].PublicIpAddress' --output text)
echo "==> Instance démarrée, IP publique : $PUBLIC_IP"

# 4. Attente : User data installe Docker, démarre les conteneurs, puis Caddy obtient le certificat (3 à 5 minutes)
URL="https://${PUBLIC_IP//./-}.sslip.io"
echo -n "==> Attente de l'application"
for _ in $(seq 1 60); do
  if curl -fs -m 5 "$URL/health" >/dev/null 2>&1; then
    echo
    echo "==> Le Guide H×H est en ligne : $URL"
    curl -s "$URL/health"
    echo
    echo "==> SSH : ssh -i $KEY_NAME.pem ec2-user@$PUBLIC_IP"
    exit 0
  fi
  echo -n "."
  sleep 10
done

echo
echo "L'application ne répond pas après 10 minutes. Diagnostic :" >&2
echo "  ssh -i $KEY_NAME.pem ec2-user@$PUBLIC_IP 'sudo cat /var/log/cloud-init-output.log'" >&2
exit 1
