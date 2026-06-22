$ErrorActionPreference = 'Stop'

$frontendRoot = $PSScriptRoot
$project = Join-Path $frontendRoot 'Interrogation.Client\Interrogation.Client.csproj'
$output = Join-Path $frontendRoot 'publish\win-x64'
$gitBin = 'C:\Program Files\Git\usr\bin'
$opensslFiles = @('openssl.exe', 'msys-crypto-3.dll', 'msys-ssl-3.dll', 'msys-2.0.dll')

foreach ($name in $opensslFiles) {
    $source = Join-Path $gitBin $name
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Не найден компонент OpenSSL: $source"
    }
}

dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o $output
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish завершился с кодом $LASTEXITCODE"
}

$opensslOutput = Join-Path $output 'openssl'
New-Item -ItemType Directory -Path $opensslOutput -Force | Out-Null
foreach ($name in $opensslFiles) {
    Copy-Item -LiteralPath (Join-Path $gitBin $name) -Destination $opensslOutput -Force
}

Write-Output "Публикация готова: $output"
