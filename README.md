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

## Ugeplaner

Ved oprettelse af en fuldtidsansættelse oprettes en ugeplan automatisk fra `FullTimeWorkSchedule` i `server/Api/appsettings.json`. Tiderne er 08.00–15.30 mandag til torsdag og 08.00–15.00 fredag. Ændringer i konfigurationen bruges ved nye oprettelser. Eksisterende planer beholder deres tider.

Ved en deltidsansættelse kræver `POST /api/users/{userId}/employments` feltet `days` med fem hverdage. Fuldtid kan også oprettes med en konkret plan i `days`. Hver dag har `day` (1–5), `isDayOff`, `startTime` og `endTime`. Fridage har tomme tider. Planen og ansættelsen gemmes i samme transaktion.

Admin og regnskab kan læse og ændre alle ugeplaner. Medarbejdere kan læse egne ansættelser, planer og arbejdstider. Skriveadgang til ugeplaner kræver Admin eller Accounting.

Under `/api/employments/{employmentId}/work-schedules` findes følgende handlinger.

| Handling | Brug |
| --- | --- |
| GET | Hent ansættelsens planer |
| GET /for-date?date=2027-01-01 | Hent planen på en dato |
| GET /{workScheduleId}/days | Hent fem hverdage med tider |
| POST | Opret en plan med startDate, endDate og days |
| PUT /{workScheduleId} | Skift tider fra startDate med nye days |
| PATCH /{workScheduleId}/end | Afslut planen med endDate |

PUT bevarer den tidligere plan indtil dagen før ændringsdatoen og gemmer en ny version fra ændringsdatoen. Slutdatoer tæller med. Perioder må ikke overlappe. Fejl under ændringen ruller hele ændringen tilbage. Når en ansættelse afsluttes, afsluttes dens resterende ugeplaner også. En plan, der først starter efter ansættelsens ønskede slutdato, skal afklares før ansættelsen kan afsluttes.

Datoopslaget og de tilhørende dage giver ferie- og saldoberegninger i #19 arbejdstiden på en bestemt dato. #19 omfatter selve beregningen. Allerede oprettede ansættelser fra før ugeplansfunktionen skal have en plan oprettet særskilt.

Kør unit tests og integrationstests fra projektroden.

```powershell
dotnet test server/BusinessEsbjerg.sln -c Release
```
