#!/usr/bin/env bash
# Base de datos primero: genera el DbContext y las entidades en Veterinaria.Datos/Models.
# Después las entidades se mueven a Veterinaria.Entidades (ver comandos.txt).
set -euo pipefail
cd "$(dirname "$0")/.."

[ -f scripts/.env ] && source scripts/.env
: "${SA_PASSWORD:?Crea scripts/.env con SA_PASSWORD=tu_password}"

# .NET instalado por snap: la herramienta global necesita DOTNET_ROOT y ~/.dotnet/tools en el PATH
if [ -d /var/snap/dotnet/common/dotnet ]; then
  export DOTNET_ROOT="${DOTNET_ROOT:-/var/snap/dotnet/common/dotnet}"
fi
export PATH="$PATH:$HOME/.dotnet/tools"

dotnet ef dbcontext scaffold \
  "Server=localhost,1433;Database=VeterinariaDB;User Id=sa;Password=$SA_PASSWORD;TrustServerCertificate=True" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --project Veterinaria.Datos --startup-project Veterinaria.Api \
  --output-dir Models --context VeterinariaContext --no-onconfiguring --force
