param(
    # Deletes the mod's existing install
    [switch]$WipeInstall
)

$ErrorActionPreference = "Stop"

# ----------------------------------------
# env check
# ----------------------------------------

$envFile = Join-Path $PSScriptRoot ".env"
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()

        if ($line -and !$line.StartsWith("#")) {
            $name, $value = $line -split '=', 2
            Set-Item -Path "Env:$($name.Trim())" -Value $value.Trim()
        }
    }
}

$sptPath = $env:TARGET_INSTALL_LOCATION
if (-not $sptPath) {
    throw "TARGET_INSTALL_LOCATION environment variable is not set."
}
if (-not (Test-Path $sptPath -PathType Container)) {
    throw "TARGET_INSTALL_LOCATION directory does not exist: $sptPath"
}

# ----------------------------------------
# build
# ----------------------------------------

$projectFile = Join-Path $PSScriptRoot "BackpackResizer.csproj"
if (-not (Test-Path $projectFile -PathType Leaf)) {
    throw "Project file does not exist: $projectFile"
}

Write-Host "Building Release..."

dotnet build $projectFile -c Release

if ($LASTEXITCODE -ne 0) {
    throw "Release build failed with exit code $LASTEXITCODE."
}
Write-Host "Build complete."

# ----------------------------------------
# find zip
# ----------------------------------------

$releaseZipPath = Join-Path $PSScriptRoot "ReleaseZip"
if (-not (Test-Path $releaseZipPath -PathType Container)) {
    throw "ReleaseZip directory does not exist: $releaseZipPath"
}

$zip = Get-ChildItem -Path $releaseZipPath -Filter "*.zip" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

if (-not $zip) {
    throw "No ZIP file found in: $releaseZipPath"
}
Write-Host "Using ZIP: $($zip.FullName)"

# ----------------------------------------
# extract zip and move files
# ----------------------------------------

$tempPath = Join-Path $env:TEMP "BackpackGridResizer_Deploy"
if (Test-Path $tempPath) {
    Remove-Item $tempPath -Recurse -Force
}

try {
    Write-Host "Extracting ZIP..."

    Expand-Archive `
        -Path $zip.FullName `
        -DestinationPath $tempPath `
        -Force

    $runtimePath = Join-Path $tempPath "SPT_Runtime"

    if (-not (Test-Path $runtimePath -PathType Container)) {
        throw "ZIP does not contain an SPT_Runtime folder."
    }

    # ----------------------------------------
    # wipe existing install (optional)
    # ----------------------------------------

    if ($WipeInstall) {
        $modFolderName = (Get-ChildItem (Join-Path $runtimePath "user\mods") -Directory | Select-Object -First 1).Name
        if (-not $modFolderName) {
            throw "Could not determine the mod's install folder name from the ZIP."
        }

        $modInstallPath = Join-Path $sptPath "SPT_Runtime\user\mods\$modFolderName"
        if (Test-Path $modInstallPath) {
            Write-Host "Wiping existing install at $modInstallPath..."
            Remove-Item $modInstallPath -Recurse -Force
        }
    }

    # ----------------------------------------
    # move mod files to game install
    # ----------------------------------------

    Write-Host "Copying files to $sptPath..."

    Copy-Item `
        -Path $runtimePath `
        -Destination $sptPath `
        -Recurse `
        -Force
}
finally {
    if (Test-Path $tempPath) {
        Remove-Item $tempPath -Recurse -Force
    }
    
    Write-Host "all done."
}