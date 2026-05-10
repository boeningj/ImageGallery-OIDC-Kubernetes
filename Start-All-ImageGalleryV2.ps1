# Start-All-ImageGalleryV2.ps1
# Portable script to launch ImageGallery.API and ImageGallery.Client
# Works even if the folder path has spaces

# ----------------------------------------
# Load environment variables from .env file
# ----------------------------------------
function Get-EnvVars {
    param ([string]$EnvFilePath)

    $envVars = @{}

    if (Test-Path $EnvFilePath) {
        Get-Content $EnvFilePath | ForEach-Object {
            if ($_ -and $_ -notmatch '^\s*#') {
                $name, $value = $_ -split '=', 2
                $envVars[$name.Trim()] = $value.Trim()
            }
        }
        Write-Host "Loaded env vars from $EnvFilePath"
    }
    else {
        Write-Warning "Env file not found: $EnvFilePath"
    }

    return $envVars
}

# ----------------------------------------
# Wait for port to respond
# ----------------------------------------
function Wait-ForPort {
    param (
        [string]$HostName = "localhost",
        [int]$Port,
        [string]$ServiceName
    )

    Write-Host "Waiting for $ServiceName on port $Port..."

    while (-not (Test-NetConnection -ComputerName $HostName -Port $Port -WarningAction SilentlyContinue).TcpTestSucceeded) {
        Start-Sleep -Seconds 1
    }

    Write-Host "$ServiceName is up!"
}

# ----------------------------------------
# Start a project in a new terminal window
# ----------------------------------------
function Start-Project {
    param (
        [string]$RelativePath,
        [string]$Name,
        [hashtable]$EnvVars
    )

    $FullPath = Join-Path $PSScriptRoot $RelativePath
    Write-Host "Starting $Name at $FullPath..."

    # Build environment variable injection string
    $envString = ""
    foreach ($key in $EnvVars.Keys) {
        $value = $EnvVars[$key].Replace("'", "''")
        $envString += "`$env:$key='$value'; "
    }

    # Full command to run in new window
    $command = "$envString `$Host.UI.RawUI.WindowTitle = '$Name'; cd '$FullPath'; dotnet run"

    Start-Process powershell -ArgumentList "-NoExit", "-Command", $command
}

# ----------------------------------------
# Load .env.local
# ----------------------------------------
$envFile = Join-Path $PSScriptRoot "env\local.env"
$envVars = Get-EnvVars $envFile

# ----------------------------------------
# Start all services
# ----------------------------------------

# Start ImageGallery.IDP
Start-Project "ImageGallery.IDP" "ImageGallery IDP" $envVars

# Wait for IDP to be ready
Wait-ForPort -Port 5001 -ServiceName "ImageGallery IDP"

# Start ImageGallery API
Start-Project "ImageGallery.API" "ImageGallery API" $envVars

# Wait for API
Wait-ForPort -Port 7075 -ServiceName "ImageGallery API"

# Start ImageGallery Client
Start-Project "ImageGallery.Client" "ImageGallery Client" $envVars

Write-Host "All projects launched!"