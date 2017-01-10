<#
.SYNOPSIS
Builds and runs a Docker image.
.PARAMETER Clean
Removes the image csclassroom.webapp and kills all containers based on that image.
.PARAMETER Build
Builds a Docker image.
.PARAMETER Run
Builds the image and runs docker-compose.
.PARAMETER StartDebugging
Finds the running container and starts the debugger inside of it.
.PARAMETER Environment
The enviorment to build for (Debug or Release), defaults to Debug
.EXAMPLE
C:\PS> .\dockerTask.ps1 -Build
Build a Docker image named csclassroom.webapp
#>

Param(
    [Parameter(Mandatory=$True,ParameterSetName="Clean")]
    [switch]$Clean,
    [Parameter(Mandatory=$True,ParameterSetName="Build")]
    [switch]$Build,
    [Parameter(Mandatory=$True,ParameterSetName="Run")]
    [switch]$Run,
    [Parameter(Mandatory=$True,ParameterSetName="StartDebugging")]
    [switch]$StartDebugging,
    [parameter(ParameterSetName="Clean")]
    [parameter(ParameterSetName="Build")]
    [Parameter(ParameterSetName="Run")]
    [parameter(ParameterSetName="StartDebugging")]
    [ValidateNotNullOrEmpty()]
    [String]$Environment = "Debug"
)

$imageName="csclassroom.webapp"
$projectName="csclassroomwebapp"
$serviceName="csclassroom.webapp"
$containerName="${projectName}_${serviceName}_1"
$runtimeID = "debian.8-x64"
$framework = "netcoreapp1.1"

# Kills all running containers of an image and then removes them.
function CleanAll () {
    if (Test-Path $composeFileName) {
        docker-compose -f "$targetFolder\docker-compose.yml" -f "$targetFolder\$composeFileName" -p $projectName down --rmi all

        $danglingImages = $(docker images -q --filter 'dangling=true')
        if (-not [String]::IsNullOrWhiteSpace($danglingImages)) {
            docker rmi -f $danglingImages
        }
    }
    else {
        Write-Error -Message "$Environment is not a valid parameter. File '$composeFileName' does not exist." -Category InvalidArgument
    }
}

# Builds the Docker image.
function BuildImage () {
    if (Test-Path $composeFileName) {
        Write-Host "Building the project ($ENVIRONMENT)."
   
        if ($Environment -eq "Debug") {
            npm run gulp-Debug
            dotnet build -f $framework -r $runtimeID -c $Environment
            if ($lastExitCode -eq 0) {
                if (-Not(Test-Path "obj\Docker\empty")) {
                    New-Item "obj\Docker\empty" -ItemType Directory
                }
            }
            else {
                $global:buildFailure = $true
            }
        }
        else {
            dotnet publish -f $framework -r $runtimeID -c $Environment -o $targetFolder
            if ($lastExitCode -ne 0) {
                $global:buildFailure = $true
            }
        }

        if (!$global:buildFailure) {
            Write-Host "Building the image $imageName ($Environment)."
            docker-compose -f "$targetFolder\docker-compose.yml" -f "$targetFolder\$composeFileName" -p $projectName build
        }
        else {
            Write-Host "Project build failed. Skipping container build."
        }
    }
    else {
        Write-Error -Message "$Environment is not a valid parameter. File '$composeFileName' does not exist." -Category InvalidArgument
    }
}

# Runs docker-compose.
function Compose () {
    if (Test-Path $composeFileName) {
        if (!$global:buildFailure) {
            if ($Environment -eq "Debug") {
                if (-Not(Test-Path "obj\Docker\empty")) {
                    New-Item "obj\Docker\empty" -ItemType Directory
                }
            }

            Write-Host "Running compose file $composeFileName"
            docker-compose -f "$targetFolder\docker-compose.yml" -f "$targetFolder\$composeFileName" -p $projectName kill
            docker-compose -f "$targetFolder\docker-compose.yml" -f "$targetFolder\$composeFileName" -p $projectName up -d
        }
    }
    else {
        Write-Error -Message "$Environment is not a valid parameter. File '$dockerFileName' does not exist." -Category InvalidArgument
    }
}

function StartDebugging () {
    Write-Host "Starting the remote debugger..."

    $containerId = (docker ps -f "name=$containerName" -q -n=1)
    if ([System.String]::IsNullOrWhiteSpace($containerId)) {
        Write-Error "Could not find a container named $containerName"
    }

    docker exec -i $containerId pkill dotnet       
    docker exec -i $containerId /clrdbg2/clrdbg --interpreter=mi
}

$global:buildFailure = $false

if($Environment -ne "Debug" -and $Environment -ne "Release") {
    Write-Error "Environment must be Debug or Release."
}

Push-Location $PSScriptRoot
[Environment]::CurrentDirectory = $PWD

if ($Environment -eq "Debug") {
    $env:TAG = ":Debug"
}

$EnvironmentLower = $Environment.ToLowerInvariant()
$composeFileName = "docker-compose.dev.$EnvironmentLower.yml"
$targetFolder = "."
if($Environment -ne "Debug") {
    $targetFolder = "bin\$Environment\$framework\publish"
}

# Call the correct function for the parameter that was used
if ($Clean) {
    CleanAll
}
elseif($Build) {
    BuildImage
}
elseif($Run) {
    BuildImage
    Compose
}
elseif($StartDebugging) {
    StartDebugging
}

Pop-Location
[Environment]::CurrentDirectory = $PWD

if ($global:buildFailure) {
    exit 1
}