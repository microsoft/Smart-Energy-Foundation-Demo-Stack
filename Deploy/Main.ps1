$ErrorActionPreference = "Stop"
if ($PSVersionTable.PSVersion.Major -lt 5)
{
    Write-Error "Powershell version 5 is required to run this script. Please update before running again."
}

# CONNECT TO AZURE RM
if ([string]::IsNullOrEmpty($(Get-AzureRmContext).Account)) {Login-AzureRmAccount}

# IMPORT helper functions
if((Test-Path -Path ".\PowershellHelperFunctions") -eq $false)
{
    Write-Error "Invalid folder structure. It seems that the PowershellHelperFunctions folder is not present. Please navigate to the root of the script deployment folder. Then kick off the script again."
}

. .\PowershellHelperFunctions\AzureHelpers.ps1
. .\PowershellHelperFunctions\InputHelpers.ps1
. .\PowershellHelperFunctions\MiscHelpers.ps1
. .\PowershellHelperFunctions\SSLPSWorkaround.ps1
. .\Deploy.ps1

# STEP 1: INPUTS (set $USE_DEFAULT_PARAM_VALUES to $true if you wish to not be prompted for input values besides deployment location and name)
$USE_DEFAULT_PARAM_VALUES = $true

## ALLOWED VALUES (Default values should be placed as first element in array. These should match those listed in the ARM template parameters)
## EXAMPLE if needed: $allowedHostingPlanSku = "S2", "B1", "B2", "B3", "S1", "S3", "P1", "P2", "P3", "P1v2", "P2v2", "P3v2"

## REGEXES for inputs
$deploymentNameRegex = "^[a-z][a-z0-9]{2,8}$"
$sqlUsernameRegex = "^[a-zA-Z][a-zA-Z0-9_]{2,}$"
$anyRegex = "^[\S]+$"

## ARM Templates for this solution
$armTemplates = @("arm\armtemplate1.json", "arm\armtemplate2.json")
$parameters = @{} # ARM template parameters dictionary. Key is ARM template relative path
for($idx = 0; $idx -lt $armTemplates.Count; $idx++)
{
    $parameters.Add($armTemplates[$idx], @{})
}

## Set parameters dictionary for each ARM template to be deployed
if($USE_DEFAULT_PARAM_VALUES)
{
    $parameters[$armTemplates[0]].Add("sqlServerUsername", "abcdefg")
    $parameters[$armTemplates[0]].Add("sqlServerPassword", "Passw0rd-2018")    
}
else
{
    $sqlUsername = GetValidInputWithRegex "sql server username" $sqlUsernameRegex 
    $sqlPassword = GetValidInputWithRegex "sql server password" $anyRegex

    $parameters[$armTemplates[0]].Add("sqlServerUsername", $sqlUsername)
    $parameters[$armTemplates[0]].Add("sqlServerPassword", $sqlPassword)

    # NOTE - "arm\armtemplate2.json" ARM template does have a parameter required "siteConfig"
    #        but since this is based on outputs from the first arm template, we will set it
    #        manually from within Deploy.ps1 and add it to the $parameters dictionary
}

## Parsing resource types from the arm templates in this solution
$resourceTypes = GetResourceTypesForTemplate $armTemplates
Write-Output "`r`n" $resourceTypes "`r`n"

## Getting the likely supported locations for deployment of this solution based
## on the resource types listed in the arm templates and the user's subscription.
$likelySupportedLocations = GetLikelyLocationsForTemplate($resourceTypes)

## Allow user to select deployment location from the list of likely supported locations
$deploymentLocation = GetValidInput "deployment location" $likelySupportedLocations

## This solution uses function so set the function names here and reference them in 
## Deploy.ps1 when having to invoke the function
$functionPrepSqlName = "prepsql"

# STEP 2: KICK OFF DEPLOYMENT
Deploy

