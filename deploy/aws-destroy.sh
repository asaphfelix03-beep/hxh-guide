#!/bin/bash
# Supprime les ressources AWS créées par aws-deploy.sh (à lancer après la notation pour éviter les frais).
# Usage : ./deploy/aws-destroy.sh
set -euo pipefail
export MSYS_NO_PATHCONV=1

NAME="hxh"
REGION="$(aws configure get region || true)"
export AWS_DEFAULT_REGION="${REGION:-us-east-1}"

INSTANCE_IDS=$(aws ec2 describe-instances \
  --filters Name=tag:Name,Values="$NAME-server" Name=instance-state-name,Values=pending,running,stopping,stopped \
  --query 'Reservations[].Instances[].InstanceId' --output text)
if [ -n "$INSTANCE_IDS" ]; then
  echo "==> Résiliation de l'instance $INSTANCE_IDS"
  # shellcheck disable=SC2086
  aws ec2 terminate-instances --instance-ids $INSTANCE_IDS >/dev/null
  # shellcheck disable=SC2086
  aws ec2 wait instance-terminated --instance-ids $INSTANCE_IDS
fi

SG_ID=$(aws ec2 describe-security-groups --filters Name=group-name,Values="$NAME-sg" \
  --query 'SecurityGroups[0].GroupId' --output text)
if [ "$SG_ID" != "None" ]; then
  echo "==> Suppression du Security Group $SG_ID"
  aws ec2 delete-security-group --group-id "$SG_ID"
fi

if aws iam get-instance-profile --instance-profile-name "$NAME-ssm-profile" >/dev/null 2>&1; then
  echo "==> Suppression du rôle IAM $NAME-ssm-role"
  aws iam remove-role-from-instance-profile --instance-profile-name "$NAME-ssm-profile" --role-name "$NAME-ssm-role"
  aws iam delete-instance-profile --instance-profile-name "$NAME-ssm-profile"
  aws iam detach-role-policy --role-name "$NAME-ssm-role" --policy-arn arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore
  aws iam delete-role --role-name "$NAME-ssm-role"
fi

if aws ec2 describe-key-pairs --key-names "$NAME-key" >/dev/null 2>&1; then
  echo "==> Suppression de la paire de clés $NAME-key"
  aws ec2 delete-key-pair --key-name "$NAME-key"
fi

echo "==> Terminé : plus aucune ressource du Guide H×H sur AWS."
