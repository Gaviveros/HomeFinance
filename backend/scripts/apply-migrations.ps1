$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($env:HOMEFINANCE_CONNECTION_STRING)) {
    throw 'Defina HOMEFINANCE_CONNECTION_STRING con una cadena de conexión no productiva o aprobada antes de aplicar migraciones.'
}

$project = Join-Path $PSScriptRoot '..\src\HomeFinance.Infrastructure\HomeFinance.Infrastructure.csproj'
dotnet ef database update --project $project --startup-project $project
if ($LASTEXITCODE -ne 0) {
    throw "La aplicación de migraciones falló con código $LASTEXITCODE."
}
