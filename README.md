# JobManager
Job Execution Controller Library

## Zentrale Verwaltung der NuGet-Versionen

Dieses Repository nutzt NuGet Central Package Management (CPM), um alle Paketversionen an einem Ort festzulegen.

- Zentrale Datei: `Directory.Packages.props` im Repository-Root
- Wirkung: Alle Projekte unterhalb erben die dort definierten Versionen. In den Projektdateien wird deshalb keine `Version="…"` mehr angegeben.

### Paketversion hinzufügen/ändern

1. Öffne `Directory.Packages.props` und füge/ändere einen Eintrag:

	```xml
	<ItemGroup>
	  <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
	  <!-- weitere Pakete hier ... -->
	</ItemGroup>
	```

2. Referenz im Projekt ohne Version eintragen/stehen lassen:

	```xml
	<PackageReference Include="Microsoft.NET.Test.Sdk" />
	```

3. Restore/Build/Test ausführen.

### Lokale Abweichung (nur ausnahmsweise)

In einem Projekt kann eine zentrale Version mit `VersionOverride` überschrieben werden:

```xml
<PackageReference Include="NUnit" VersionOverride="4.3.2" />
```

### Transitive Pakete pinnen

Die Datei ist mit `CentralPackageTransitivePinningEnabled` konfiguriert, damit transitive Pakete auf zentral gesetzte Versionen gepinnt werden können.

Weiterführende Infos: https://learn.microsoft.com/nuget/consume-packages/Central-Package-Management

