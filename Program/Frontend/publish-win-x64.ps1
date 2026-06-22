$ErrorActionPreference = 'Stop'

$frontendRoot = $PSScriptRoot
$project = Join-Path $frontendRoot 'Interrogation.Client\Interrogation.Client.csproj'
$output = Join-Path $frontendRoot 'publish\win-x64'

dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $output

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish завершился с кодом $LASTEXITCODE"
}

Write-Output "Windows-публикация готова: $output"
