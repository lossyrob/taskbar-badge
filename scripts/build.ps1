param(
    [string]$Configuration = "Release",
    [string]$Version = "0.1.0",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$repo = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $repo "artifacts"
$publishDir = Join-Path $artifacts "publish\TaskbarBadge"
$packageDir = Join-Path $artifacts "packages"
$assemblyVersion = ($Version -replace "-.*$", "")

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Command
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code $LASTEXITCODE"
    }
}

Remove-Item $artifacts -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $publishDir, $packageDir -Force | Out-Null

Push-Location $repo
try {
    Invoke-Checked { dotnet restore .\TaskbarBadge.slnx }
    Invoke-Checked {
        dotnet build .\TaskbarBadge.slnx `
            --configuration $Configuration `
            -p:Version=$Version `
            -p:AssemblyVersion=$assemblyVersion `
            -p:FileVersion=$assemblyVersion `
            --no-restore
    }
    Invoke-Checked {
        dotnet test .\TaskbarBadge.slnx `
            --configuration $Configuration `
            -p:Version=$Version `
            -p:AssemblyVersion=$assemblyVersion `
            -p:FileVersion=$assemblyVersion `
            --no-build
    }

    Invoke-Checked {
        dotnet publish .\src\TaskbarBadge.App\TaskbarBadge.App.csproj `
            --configuration $Configuration `
            --runtime win-x64 `
            --self-contained true `
            --output $publishDir `
            -p:Version=$Version `
            -p:AssemblyVersion=$assemblyVersion `
            -p:FileVersion=$assemblyVersion `
            -p:PublishSingleFile=true `
            -p:IncludeNativeLibrariesForSelfExtract=true
    }

    $zip = Join-Path $packageDir "TaskbarBadge-$Version-win-x64-portable.zip"
    Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zip -Force

    if (-not $SkipInstaller) {
        Invoke-Checked {
            dotnet build .\installer\TaskbarBadge.wixproj `
                --configuration $Configuration `
                -p:ProductVersion=$assemblyVersion `
                -p:PublishDir="$publishDir\"
        }

        Get-ChildItem .\installer\bin\$Configuration -Filter "*.msi" -Recurse |
            Copy-Item -Destination $packageDir -Force
    }

    $checksumPath = Join-Path $packageDir "checksums.txt"
    Get-ChildItem $packageDir -File |
        Where-Object { $_.Name -ne "checksums.txt" } |
        Sort-Object Name |
        ForEach-Object {
            $hash = Get-FileHash $_.FullName -Algorithm SHA256
            "$($hash.Hash.ToLowerInvariant())  $($_.Name)"
        } |
        Set-Content -Path $checksumPath -Encoding utf8

    Write-Host "Artifacts written to $packageDir"
}
finally {
    Pop-Location
}
