#!/bin/bash
# Imports n8n workflow JSON files into the running n8n container.
#
# Prerequisites:
#   1. Infrastructure containers running: npm run infra:up
#   2. n8n setup wizard completed at http://localhost:5678/setup
#
# Usage:
#   ./scripts/import-n8n-workflows.sh
#
# After import, go to http://localhost:5678/workflows and activate each workflow.

set -e

CONTAINER=atm-n8n
WORKFLOW_DIR=/home/node/workflows

echo "Importing n8n workflows from $WORKFLOW_DIR into container $CONTAINER..."

if ! docker ps --format '{{.Names}}' | grep -q "^$CONTAINER$"; then
  echo "Error: Container $CONTAINER is not running. Run 'npm run infra:up' first."
  exit 1
fi

docker exec "$CONTAINER" n8n import:workflow --separate --input="$WORKFLOW_DIR"

echo ""
echo "Done. Open http://localhost:5678/workflows to see the imported workflows."
echo "Remember to activate each scheduled workflow to start the demo traffic."
