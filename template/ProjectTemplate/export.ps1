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
$projectPath = Join-Path $scriptDir $config.projectPath
$templateName = $config.templateName
$outputDir = Join-Path $scriptDir $exportConfig.outputDir
$vsTemplatePath = Invoke-Expression "`"$($exportConfig.vsTemplatePath)`""  # Evaluate environment variables
$vstemplateSource = Join-Path $configDir $config.vstemplateSource
$iconSource = Join-Path $configDir $config.iconSource
$namespaceToReplace = $config.namespaceToReplace

# Convert paths to absolute paths
$configDir = [System.IO.Path]::GetFullPath($configDir)
$projectPath = [System.IO.Path]::GetFullPath($projectPath)
$vstemplateSource = [System.IO.Path]::GetFullPath($vstemplateSource)
$iconSource = [System.IO.Path]::GetFullPath($iconSource)

# Debug: Log configuration paths
Write-Output "Script Directory: $scriptDir"
Write-Output "ConfigPath:       $ConfigPath"
Write-Output "Config Directory: $configDir"
Write-Output "vstemplateSource: $vstemplateSource"
Write-Output "iconSource:       $iconSource"
Write-Output "outputDir:        $outputDir"
Write-Output "projectPath:      $projectPath"
Write-Output "vsTemplatePath:   $vsTemplatePath"

# Verify project folder exists
if (-not (Test-Path $projectPath)) {
    Write-Error "Project folder not found: $projectPath"
    exit
}

# Verify vstemplate file
if (-not (Test-Path $vstemplateSource)) {
    Write-Error ".vstemplate file not found at: $vstemplateSource"
    Write-Error "Please check config.json 'vstemplateSource' and ensure the file exists in $configDir"
    exit
}

# Verify icon file
if (-not (Test-Path $iconSource)) {
    Write-Error "Template icon file not found at: $iconSource"
    Write-Error "Please check config.json 'iconSource' and ensure the file exists in $configDir"
    exit
}

# Create output directory
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# Create temporary folder for staging (excluding bin, obj)
$tempDir = Join-Path $outputDir "Temp_$templateName"
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Copy project files, excluding bin, obj, and *.csproj.user, with namespace replacement
Get-ChildItem -Path $projectPath -Recurse -Exclude @("bin", "obj", "*.csproj.user") |
    Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" -and $_.Extension -ne ".csproj.user" } |
    ForEach-Object {
        # Calculate relative path from project folder
        $relativePath = $_.FullName.Substring($projectPath.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar)
        $destPath = Join-Path $tempDir $relativePath
        $destDir = Split-Path $destPath -Parent
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir | Out-Null
        }
        if (-not $_.PSIsContainer) {
            if ($_.Extension -in @(".cs", ".csproj", ".xaml")) {
                # Read file content with UTF-8 encoding (no BOM)
                $content = [System.IO.File]::ReadAllText($_.FullName, [System.Text.UTF8Encoding]::new($false))
                if ($_.Extension -eq ".cs") {
                    # Replace namespace in .cs files
                    $content = $content -replace "namespace\s+$namespaceToReplace\b", "namespace `$safeprojectname$"
                    # Replace using statements
                    $content = $content -replace "using\s+$namespaceToReplace\b", "using `$safeprojectname$"
                }
                elseif ($_.Extension -eq ".csproj") {
                    # Replace project name in .csproj files
                    $content = $content -replace "<RootNamespace>$namespaceToReplace</RootNamespace>", "<RootNamespace>`$safeprojectname$</RootNamespace>"
                    $content = $content -replace "<AssemblyName>$namespaceToReplace</AssemblyName>", "<AssemblyName>`$safeprojectname$</AssemblyName>"
                    $content = $content -replace "<ApplicationTitle>$namespaceToReplace</ApplicationTitle>", "<ApplicationTitle>`$safeprojectname$</ApplicationTitle>"
                    # Comment out all ProjectReference elements
                    $content = $content -replace "<ProjectReference Include=`".*?`" />", "<!--`$0-->"
                }
                elseif ($_.Extension -eq ".xaml") {
                    # Replace namespace in .xaml files
                    $content = $content -replace $namespaceToReplace, "`$safeprojectname$"
                }
                # Write modified content with UTF-8 encoding (no BOM)
                [System.IO.File]::WriteAllText($destPath, $content, [System.Text.UTF8Encoding]::new($false))
            }
            else {
                # Copy other files without modification
                Copy-Item -Path $_.FullName -Destination $destPath
            }
        }
    }

# Copy .vstemplate file with UTF-8 encoding (no BOM)
$vstemplateDest = Join-Path $tempDir "$templateName.vstemplate"
$vstemplateContent = [System.IO.File]::ReadAllText($vstemplateSource, [System.Text.UTF8Encoding]::new($false))
[System.IO.File]::WriteAllText($vstemplateDest, $vstemplateContent, [System.Text.UTF8Encoding]::new($false))

# Copy template icon file
$iconDest = Join-Path $tempDir "__TemplateIcon.ico"
Copy-Item -Path $iconSource -Destination $iconDest

# Create ZIP file
$zipPath = Join-Path $outputDir "$templateName.zip"
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath
}
Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath

# Copy ZIP to Visual Studio C# template folder
if (-not (Test-Path $vsTemplatePath)) {
    New-Item -ItemType Directory -Path $vsTemplatePath | Out-Null
}
$vsTemplateDest = Join-Path $vsTemplatePath "$templateName.zip"
Copy-Item -Path $zipPath -Destination $vsTemplateDest

# Clean up temporary folder
Remove-Item -Recurse -Force $tempDir

Write-Output "Template '$templateName.zip' created at $outputDir and copied to Visual Studio C# template folder."
Write-Output "Execute the following command to refresh template cache: devenv /updateconfiguration"