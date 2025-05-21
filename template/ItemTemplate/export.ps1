param (
    [Parameter(Mandatory=$true)]
    [string]$ConfigPath
)

# Load export configuration from exportConfig.json
$scriptDir = $PSScriptRoot  # Directory of this script
$exportConfigPath = Join-Path $scriptDir "export.json"
if (-not (Test-Path $exportConfigPath)) {
    Write-Error "Export configuration file not found: $exportConfigPath"
    exit
}
$exportConfig = Get-Content -Path $exportConfigPath -Raw | ConvertFrom-Json

# Resolve ConfigPath to absolute path
$ConfigPath = [System.IO.Path]::GetFullPath((Join-Path $scriptDir $ConfigPath))

# Load project configuration from specified config.json
if (-not (Test-Path $ConfigPath)) {
    Write-Error "Project configuration file not found: $ConfigPath"
    exit
}
$config = Get-Content -Path $ConfigPath -Raw | ConvertFrom-Json

# Resolve configuration paths
$configDir = Split-Path -Parent $ConfigPath  # Directory of config.json
$templateName = $config.templateName
$outputDir = Join-Path $scriptDir $exportConfig.outputDir
$vsTemplatePath = Invoke-Expression "`"$($exportConfig.vsTemplatePath)`""  # Evaluate environment variables

# Convert paths to absolute paths
$configDir = [System.IO.Path]::GetFullPath($configDir)
$outputDir = [System.IO.Path]::GetFullPath($outputDir)

# Debug: Log configuration paths
Write-Output "Script Directory: $scriptDir"
Write-Output "ConfigPath:       $ConfigPath"
Write-Output "Config Directory: $configDir"
Write-Output "templateName:     $templateName"
Write-Output "outputDir:        $outputDir"
Write-Output "vsTemplatePath:   $vsTemplatePath"

# Verify templateName exists
if (-not $templateName) {
    Write-Error "templateName is not specified in config.json"
    exit
}

# Create output directory
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# Create temporary folder for staging
$tempDir = Join-Path $outputDir "Temp_$templateName"
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Copy all files and folders from configDir, excluding config.json
Get-ChildItem -Path $configDir -Exclude "config.json" |
    ForEach-Object {
        $destPath = Join-Path $tempDir $_.Name
        if ($_.PSIsContainer) {
            Copy-Item -Path $_.FullName -Destination $destPath -Recurse
        }
        else {
            if ($_.Extension -eq ".vstemplate") {
                # Copy .vstemplate file with UTF-8 encoding (no BOM)
                $content = [System.IO.File]::ReadAllText($_.FullName, [System.Text.UTF8Encoding]::new($false))
                [System.IO.File]::WriteAllText($destPath, $content, [System.Text.UTF8Encoding]::new($false))
            }
            else {
                # Copy other files without modification
                Copy-Item -Path $_.FullName -Destination $destPath
            }
        }
    }

# Create ZIP file
$zipPath = Join-Path $outputDir "$templateName.zip"
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath
}
Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath

# Copy ZIP to Visual Studio ItemTemplates folder
if (-not (Test-Path $vsTemplatePath)) {
    New-Item -ItemType Directory -Path $vsTemplatePath | Out-Null
}
$vsTemplateDest = Join-Path $vsTemplatePath "$templateName.zip"
Copy-Item -Path $zipPath -Destination $vsTemplateDest

# Clean up temporary folder
Remove-Item -Recurse -Force $tempDir

Write-Output "Item template '$templateName.zip' created at $outputDir and copied to Visual Studio C# ItemTemplates folder."
Write-Output "Execute the following command to refresh template cache: devenv /updateconfiguration"