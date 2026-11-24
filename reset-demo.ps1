# Run from the repository: .\reset-demo.ps1
# Deletes ALL data in this project's local Docker database and creates demo users.
$ErrorActionPreference = 'Stop'
$composeFile = Join-Path $PSScriptRoot 'docker-compose.yml'
$migrationFolder = Join-Path $PSScriptRoot 'server/Infra/Migrations'
$seedFile = Join-Path $PSScriptRoot 'scripts/demo-users.sql'

# Never run this against a remote Docker context or a configured cloud database.
if ($env:DOCKER_HOST -and $env:DOCKER_HOST -notmatch '^(npipe|unix)://') {
    throw 'Reset is not allowed with a remote DOCKER_HOST.'
}
$context = docker context show
if ($LASTEXITCODE -ne 0) { throw 'Docker is not available.' }
$endpoint = docker context inspect $context --format '{{.Endpoints.docker.Host}}'
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect the Docker context.' }
if ($endpoint -notmatch '^(npipe|unix)://') {
    throw 'Reset is only allowed with a local Docker context.'
}

docker info --format '{{.ServerVersion}}' | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Start Docker Desktop first.' }

# Build the complete SQL before touching the database.
$sql = "BEGIN;`nSET LOCAL lock_timeout = '10s';`n"
$sql += "SET LOCAL client_min_messages = warning;`n"
$sql += "DROP SCHEMA public CASCADE;`nCREATE SCHEMA public;`n"
$sql += "CREATE TABLE schema_migrations (version text PRIMARY KEY, applied_at timestamptz NOT NULL DEFAULT now());`n"
$migrations = @(Get-ChildItem -LiteralPath $migrationFolder -Filter '*.sql' | Sort-Object Name)
if ($migrations.Count -eq 0) { throw 'No database migrations found.' }
foreach ($migration in $migrations) {
    $sql += (Get-Content -LiteralPath $migration.FullName -Raw) + "`n"
    $version = $migration.BaseName.Replace("'", "''")
    $sql += "INSERT INTO schema_migrations (version) VALUES ('$version');`n"
}
$sql += (Get-Content -LiteralPath $seedFile -Raw) + "`nCOMMIT;`n"

docker compose -f $composeFile up -d db
if ($LASTEXITCODE -ne 0) { throw 'Could not start the local database.' }

$ready = $false
for ($attempt = 0; $attempt -lt 30; $attempt++) {
    docker compose -f $composeFile exec -T db pg_isready -U postgres -d business_esbjerg 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    Start-Sleep -Seconds 1
}
if (!$ready) { throw 'PostgreSQL did not become ready.' }

Write-Host 'Resetting ALL data in the local business_esbjerg Docker database...'
# ON_ERROR_STOP + one transaction rolls back the reset if any SQL fails.
$sql | docker compose -f $composeFile exec -T db psql -U postgres -d business_esbjerg -v ON_ERROR_STOP=1 --quiet
if ($LASTEXITCODE -ne 0) { throw 'Reset failed. The database transaction was rolled back.' }

Write-Host 'Ready. Password for all three accounts: 1234567891011'
Write-Host 'Admin:      admin@businessesbjerg.com'
Write-Host 'Accounting: accounting@businessesbjerg.com'
Write-Host 'Employee:   employee@businessesbjerg.com (full-time, 37 hours/week)'
Write-Host 'Log out and log in again. Start the API and frontend as usual.'
