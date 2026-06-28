#!/bin/bash
# Lance l'app avec le SDK .NET 10 (~/.dotnet) et prépare la base locale.
export PATH="$HOME/.dotnet:$PATH"
cd "$(dirname "$0")"

if [ ! -f GestionPersonnel.db ]; then
  echo "→ Base absente : copie de GestionPersonnel.db.backup (comptes + données de démo)"
  cp GestionPersonnel.db.backup GestionPersonnel.db
elif ! sqlite3 GestionPersonnel.db "SELECT 1 FROM pragma_table_info('Utilisateurs') WHERE name='EmailVerifie';" 2>/dev/null | grep -q 1; then
  echo "→ Base incomplète : restauration depuis GestionPersonnel.db.backup"
  rm -f GestionPersonnel.db-shm GestionPersonnel.db-wal
  cp GestionPersonnel.db.backup GestionPersonnel.db
fi

echo "→ Emails SMTP : appsettings.Development.json (actif en mode Development)"
echo "→ Compte SuperAdmin : lafiamongo@gmail.com / mot de passe : admin123"

if [ -f bin/Debug/net10.0/GestionPersonnelMairie.dll ]; then
  dotnet run --no-build "$@"
else
  dotnet run "$@"
fi
