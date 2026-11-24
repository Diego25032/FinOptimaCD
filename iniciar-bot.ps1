$ErrorActionPreference = "Stop"

$carpeta = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $carpeta

$nombres = @(
    "FINOPTIMA_TELEGRAM_TOKEN",
    "FINOPTIMA_TELEGRAM_CHAT",
    "FINOPTIMA_BOT_CORREO",
    "FINOPTIMA_BOT_CONTRASENA"
)

$faltantes = @()

foreach ($nombre in $nombres) {
    $valor = [Environment]::GetEnvironmentVariable($nombre, "User")

    if ($valor) {
        Set-Item -Path ("Env:" + $nombre) -Value $valor
    }
    else {
        $faltantes += $nombre
    }
}

$conexion = [Environment]::GetEnvironmentVariable("FINOPTIMA_CONEXION", "User")

if ($conexion) {
    Set-Item -Path "Env:FINOPTIMA_CONEXION" -Value $conexion
}

if ($faltantes.Count -gt 0) {
    Write-Host "Faltan estas variables en Windows:" -ForegroundColor Red

    foreach ($nombre in $faltantes) {
        Write-Host ("  " + $nombre) -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Configuralas con setx y vuelve a ejecutar este archivo."
    Read-Host "Presiona Enter para cerrar"
    exit 1
}

Write-Host "Variables cargadas correctamente. Iniciando FinOptima Bot..." -ForegroundColor Green
dotnet run --project FinOptima.Bot
