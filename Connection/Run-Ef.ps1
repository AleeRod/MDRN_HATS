param(
    [ValidateSet("Check", "Script")]
    [string]$Action = "Check"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$secureConnectionString = Read-Host "Pega el connection string de tu base de PRUEBAS" -AsSecureString
$passwordPointer = [IntPtr]::Zero
$previousConnectionString = $env:ConnectionStrings__DefaultConnection
$environmentDefaults = @{
    Jwt__Key = "design-time-only-key-not-used-for-token-signing"
    Jwt__Issuer = "MDRN_HATS.EF.DesignTime"
    Jwt__Audience = "MDRN_HATS.EF.DesignTime"
    Cors__AllowedOrigins = "http://localhost"
}
$previousEnvironment = @{}

try {
    $passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureConnectionString)
    $connectionString = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        throw "El connection string no puede estar vacío."
    }
    $env:ConnectionStrings__DefaultConnection = $connectionString

    foreach ($name in $environmentDefaults.Keys) {
        $previousEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, "Process")
        if ([string]::IsNullOrWhiteSpace($previousEnvironment[$name])) {
            [Environment]::SetEnvironmentVariable($name, $environmentDefaults[$name], "Process")
        }
    }

    Push-Location $repositoryRoot
    try {
        $efArguments = @(
            "--project", "backend\Infrastructure\MDRN_HATS.Infrastructure.csproj",
            "--startup-project", "backend\Api\MDRN_HATS.Api.csproj",
            "--context", "AppDbContext"
        )

        if ($Action -eq "Check") {
            $efOutput = & dotnet ef migrations list @efArguments 2>&1
            $exitCode = $LASTEXITCODE
            $efOutput | ForEach-Object { Write-Host $_ }

            if ($exitCode -ne 0 -or ($efOutput -join "`n") -match
                "(?i)error occurred while accessing the database|error al acceder a la base de datos|pending status not shown|unable to determine which migrations") {
                throw "No se pudo verificar el acceso y estado de las migraciones. No se aplicó ningún cambio."
            }

            Write-Host "Verificación completada. No se aplicaron migraciones."
        }
        else {
            $sqlPath = Join-Path $env:TEMP "MDRN_HATS-migrations-idempotent.sql"
            & dotnet ef migrations script --idempotent @efArguments --output $sqlPath
            if ($LASTEXITCODE -ne 0) {
                throw "No se pudo generar el script idempotente."
            }

            Write-Host "SQL idempotente generado en: $sqlPath"
            Write-Host "El script solo se generó; no se ejecutó contra la base de datos."
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    if ($passwordPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
    }

    $env:ConnectionStrings__DefaultConnection = $previousConnectionString
    foreach ($name in $environmentDefaults.Keys) {
        [Environment]::SetEnvironmentVariable($name, $previousEnvironment[$name], "Process")
    }

    Remove-Variable secureConnectionString -ErrorAction SilentlyContinue
    Remove-Variable connectionString -ErrorAction SilentlyContinue
}
