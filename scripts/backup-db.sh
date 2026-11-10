#!/usr/bin/env bash
set -Eeuo pipefail

: "${PGHOST:?Set PGHOST}"
: "${PGDATABASE:?Set PGDATABASE}"
: "${PGUSER:?Set PGUSER}"
: "${PGPASSWORD:?Set PGPASSWORD}"

BACKUP_DIR="${BACKUP_DIR:-./backups}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"
if [[ ! "$RETENTION_DAYS" =~ ^[0-9]+$ ]]; then
  printf 'RETENTION_DAYS must be a non-negative integer.\n' >&2
  exit 1
fi

mkdir -p "$BACKUP_DIR"

timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
backup_file="$BACKUP_DIR/mdrn_hats_${timestamp}.dump.gz"
temporary_file="$(mktemp "$BACKUP_DIR/.mdrn_hats_XXXXXXXX")"
trap 'rm -f -- "$temporary_file"' EXIT

pg_dump --no-password --format=custom | gzip -c > "$temporary_file"
mv -- "$temporary_file" "$backup_file"
find "$BACKUP_DIR" -type f -name 'mdrn_hats_*.dump.gz' -mtime "+$RETENTION_DAYS" -delete
printf 'Database backup created: %s\n' "$backup_file"
