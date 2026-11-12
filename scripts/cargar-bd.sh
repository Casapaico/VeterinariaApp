#!/usr/bin/env bash
# (Re)crea VeterinariaDB con db/VeterinariaDB.sql en el SQL Server local (puerto 1433)
set -euo pipefail
cd "$(dirname "$0")/.."

# La contraseña de sa no se sube al repo: va en scripts/.env (SA_PASSWORD=...)
[ -f scripts/.env ] && source scripts/.env
: "${SA_PASSWORD:?Crea scripts/.env con SA_PASSWORD=tu_password}"

/opt/mssql-tools18/bin/sqlcmd -C -S localhost,1433 -U sa -P "$SA_PASSWORD" -b -i db/VeterinariaDB.sql
echo "Listo: VeterinariaDB creada."
