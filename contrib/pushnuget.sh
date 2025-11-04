#!/bin/bash

# Standardwerte
source=""
api_key=""

# Argumente parsen
while [[ "$#" -gt 0 ]]; do
  case $1 in
    --source=*) source="${1#*=}" ;;
    --api-key=*) api_key="${1#*=}" ;;
    *) 
      if [ -z "$path" ]; then
        path="$1"
      else
        echo "Unbekannter Parameter oder mehrfacher Pfad: $1"
        exit 1
      fi
      ;;
  esac
  shift
done

# Überprüfen, ob ein Pfad angegeben wurde
if [ -z "$path" ]; then
  echo "Bitte einen Pfad angeben."
  exit 1
fi

# Überprüfen, ob Quelle und API-Key gesetzt sind
if [ -z "$source" ] || [ -z "$api_key" ]; then
  echo "Bitte --source und --api-key angeben."
  exit 1
fi

# Alle csproj-Dateien im angegebenen Pfad und Unterverzeichnissen finden
find "$path" -name "*.csproj" | while read csproj; do
  if grep -q "<IsPackable>true</IsPackable>" "$csproj"; then
    project_dir=$(dirname "$csproj")
    cd "$project_dir" || exit

    if ! dotnet test "$project_dir"; then
      echo "Fehler beim Testen des Projekts: $csproj"
      exit 1
    fi

    if ! dotnet build "$project_dir"; then
      echo "Fehler beim Bauen des Projekts: $csproj"
      exit 1
    fi

    dotnet nuget push "$project_dir/bin/Release/*.nupkg" --skip-duplicate --source "$source" --api-key "$api_key"
  fi
done
