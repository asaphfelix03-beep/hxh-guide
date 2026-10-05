#!/bin/bash
# Déploie le Guide H×H sur une instance EC2 avec AWS CLI v2.
# Crée (ou réutilise) : un rôle IAM minimal pour Systems Manager, un Security Group (80/443 seulement)
# et une instance t3.micro Amazon Linux 2023 qui exécute deploy/user-data.sh au premier démarrage.
#
# Sécurité : aucun port SSH ouvert ni clé SSH. L'administration passe par AWS Systems Manager
# (authentifié par IAM et journalisé). Disque chiffré, IMDSv2 obligatoire.
#
# Prérequis : AWS CLI connecté (aws login) et image publiée sur Docker Hub.
# Usage (Git Bash, Linux ou macOS) : ./deploy/aws-deploy.sh
set -euo pipefail
export MSYS_NO_PATHCONV=1   # Git Bash sous Windows : ne pas convertir les arguments "/aws/..."

cd "$(dirname "$0")/.."

NAME="hxh"
SG_NAME="$NAME-sg"
ROLE_NAME="$NAME-ssm-role"
PROFILE_NAME="$NAME-ssm-profile"
INSTANCE_NAME="$NAME-server"
INSTANCE_TYPE="${INSTANCE_TYPE:-t3.micro}"
REGION="$(aws configure get region || true)"
export AWS_DEFAULT_REGION="${REGION:-us-east-1}"

echo "==> Compte AWS $(aws sts get-caller-identity --query Account --output text), région $AWS_DEFAULT_REGION"

# 1. Rôle IAM minimal : uniquement la politique gérée nécessaire à Systems Manager.
if ! aws iam get-instance-profile --instance-profile-name "$PROFILE_NAME" >/dev/null 2>&1; then
  aws iam create-role --role-name "$ROLE_NAME" \
    --assume-role-policy-document '{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"Service":"ec2.amazonaws.com"},"Action":"sts:AssumeRole"}]}' >/dev/null
  aws iam attach-role-policy --role-name "$ROLE_NAME" --policy-arn arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore
  aws iam create-instance-profile --instance-profile-name "$PROFILE_NAME" >/dev/null
  aws iam add-role-to-instance-profile --instance-profile-name "$PROFILE_NAME" --role-name "$ROLE_NAME"
  echo "==> Rôle $ROLE_NAME créé"
  sleep 10   # propagation IAM
fi

# 2. Security Group : uniquement le web (80 pour la redirection et Let's Encrypt, 443 TCP/UDP).
VPC_ID=$(aws ec2 describe-vpcs --filters Name=is-default,Values=true --query 'Vpcs[0].VpcId' --output text)
SG_ID=$(aws ec2 describe-security-groups --filters Name=group-name,Values="$SG_NAME" Name=vpc-id,Values="$VPC_ID" \
  --query 'SecurityGroups[0].GroupId' --output text)
if [ "$SG_ID" = "None" ]; then
  SG_ID=$(aws ec2 create-security-group --group-name "$SG_NAME" --vpc-id "$VPC_ID" \
    --description "Guide HxH - web uniquement" --query GroupId --output text)
  echo "==> Security Group $SG_ID créé"
fi
for rule in tcp:80 tcp:443 udp:443; do
  aws ec2 authorize-security-group-ingress --group-id "$SG_ID" \
    --protocol "${rule%%:*}" --port "${rule##*:}" --cidr 0.0.0.0/0 >/dev/null 2>&1 || true
done

# 3. Instance (réutilisée si elle tourne déjà)
INSTANCE_ID=$(aws ec2 describe-instances \
  --filters Name=tag:Name,Values="$INSTANCE_NAME" Name=instance-state-name,Values=pending,running \
  --query 'Reservations[0].Instances[0].InstanceId' --output text)
if [ "$INSTANCE_ID" = "None" ]; then
  AMI_ID=$(aws ssm get-parameters \
    --names /aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64 \
    --query 'Parameters[0].Value' --output text)

  # Fins de ligne Linux obligatoires : un script en CRLF (édité sous Windows) ferait échouer bash.
  USER_DATA="deploy/.user-data.lf"
  tr -d '\r' < deploy/user-data.sh > "$USER_DATA"
  trap 'rm -f "$USER_DATA"' EXIT

  INSTANCE_ID=$(aws ec2 run-instances \
    --image-id "$AMI_ID" \
    --instance-type "$INSTANCE_TYPE" \
    --security-group-ids "$SG_ID" \
    --iam-instance-profile Name="$PROFILE_NAME" \
    --metadata-options HttpTokens=required,HttpEndpoint=enabled,HttpPutResponseHopLimit=1 \
    --block-device-mappings '[{"DeviceName":"/dev/xvda","Ebs":{"VolumeSize":8,"VolumeType":"gp3","Encrypted":true}}]' \
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
    echo "==> Administration : aws ssm start-session --target $INSTANCE_ID"
    exit 0
  fi
  echo -n "."
  sleep 10
done

echo
echo "L'application ne répond pas après 10 minutes. Diagnostic (journal du premier démarrage) :" >&2
echo "  aws ssm send-command --instance-ids $INSTANCE_ID --document-name AWS-RunShellScript --parameters 'commands=[\"tail -50 /var/log/cloud-init-output.log\"]'" >&2
exit 1
