# Business-Esbjerg-Absence-Resource-Planning

## Brugeradministration i lokal udvikling

Start API'et fra projektroden. Vælg en lokal admin-e-mail og adgangskode første gang.

```powershell
$env:SeedAdminEmail = Read-Host "Admin-e-mail"
$env:SeedAdminPassword = Read-Host "Admin-adgangskode" -MaskInput
dotnet run --project .\server\Api\Api.csproj
```

Åbn [Swagger](http://localhost:5159/swagger), kald `POST /api/auth/login` med adminkontoen, og brug derefter `/api/users` i samme vindue.

Kun rollen `Admin` kan liste, oprette, redigere og slette konti. Brugerne kan have rollerne `Admin`, `Employee` og `Accounting`. Ved oprettelse kræves en adgangskode på mindst 12 tegn. Redigering ændrer e-mail og rolle. En admin kan hverken slette sin egen konto eller fjerne sin egen adminrolle. Brugersvar indeholder ID, e-mail og rolle.

Adminoprettelse via miljøvariabler kører i Development. Brug en e-mail, der er ledig i den lokale database. Efter første opstart kan du fjerne de to miljøvariabler med `Remove-Item Env:SeedAdminEmail, Env:SeedAdminPassword`. Kontoen er gemt i SQLite.
