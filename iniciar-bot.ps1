$ErrorActionPreference = "Stop"

$carpeta = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $carpeta

function Obtener-Variable {
    param([string]$nombre)

    foreach ($ambito in @("Process", "User", "Machine")) {
        $valor = [Environment]::GetEnvironmentVariable($nombre, $ambito)

        if (-not [string]::IsNullOrWhiteSpace($valor)) {
            return $valor.Trim()
        }
    }

    return $null
}

function Leer-Secreto {
    param([string]$mensaje)

    $secreto = Read-Host $mensaje -AsSecureString
    $puntero = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secreto)

    try {
        return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($puntero)
    }
    finally {
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($puntero)
    }
}

$nombres = @(
    "FINOPTIMA_TELEGRAM_TOKEN",
    "FINOPTIMA_TELEGRAM_CHAT",
    "FINOPTIMA_BOT_CORREO",
    "FINOPTIMA_BOT_CONTRASENA"
)

$valores = @{}
$faltantes = @()

foreach ($nombre in $nombres) {
    $valores[$nombre] = Obtener-Variable -nombre $nombre

    if ([string]::IsNullOrWhiteSpace($valores[$nombre])) {
        $faltantes += $nombre
    }
}

if ($faltantes.Count -gt 0) {
    Write-Host "Faltan datos para activar el bot. Se guardaran como variables de entorno de tu usuario de Windows." -ForegroundColor Yellow

    foreach ($nombre in $faltantes) {
        if ($nombre -eq "FINOPTIMA_TELEGRAM_TOKEN") {
            $valores[$nombre] = Leer-Secreto -mensaje "Pega el token que te dio BotFather (no se mostrara al escribirlo)"
        }
        elseif ($nombre -eq "FINOPTIMA_TELEGRAM_CHAT") {
            $valores[$nombre] = Read-Host "Pega tu id numerico de chat de @userinfobot"
        }
        elseif ($nombre -eq "FINOPTIMA_BOT_CORREO") {
            $valores[$nombre] = Read-Host "Escribe el correo de tu cuenta de FinOptima"
        }
        else {
            $valores[$nombre] = Leer-Secreto -mensaje "Escribe la contrasena de esa cuenta (no se mostrara al escribirla)"
        }

        if ([string]::IsNullOrWhiteSpace($valores[$nombre])) {
            Write-Host ("No puede quedar vacio: " + $nombre) -ForegroundColor Red
            exit 1
        }

        $valores[$nombre] = $valores[$nombre].Trim()

        try {
            [Environment]::SetEnvironmentVariable($nombre, $valores[$nombre], "User")
        }
        catch {
            Write-Host ("No se pudo guardar " + $nombre + " en Windows.") -ForegroundColor Red
            exit 1
        }
    }

    Write-Host "Datos guardados correctamente." -ForegroundColor Green
}

foreach ($nombre in $nombres) {
    Set-Item -Path ("Env:" + $nombre) -Value $valores[$nombre]
}

$conexion = Obtener-Variable -nombre "FINOPTIMA_CONEXION"

if ($conexion) {
    Set-Item -Path "Env:FINOPTIMA_CONEXION" -Value $conexion
}

Write-Host "Variables cargadas correctamente. Iniciando FinOptima Bot..." -ForegroundColor Green
dotnet run --project FinOptima.Bot
$codigoSalida = $LASTEXITCODE

if ($codigoSalida -ne 0) {
    Write-Host ("FinOptima Bot finalizo con codigo " + $codigoSalida + ".") -ForegroundColor Red
    exit $codigoSalida
}
