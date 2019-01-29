function Deploy()
{
    # INPUTS (deployment name)
    $deploymentName = GetValidInputWithRegex "deployment name (alphanumeric from 3 to 9 characters)" $deploymentNameRegex
    Write-Output "The deployment name will be : $deploymentName. The deployment location will be $deploymentLocation"

    # Creating resource group
    Write-Output "Creating resource group $deploymentName..."
    New-AzureRmResourceGroup -Name $deploymentName -Location $deploymentLocation 

    # ARM Deployments
    
    ## outputs store (will collect ARM deployment outputs)
    $outputs = @{}

    ## ARM deployment #1 (SQL server/db and Storage Acct)
    $idx = 0    # increment manually at each ARM deployment
    $azureDeploymentName = $deploymentName + "-" + $idx
    $armTemplate = $armTemplates[$idx]
    Write-Output "Deploying via ARM template, $armTemplate, into Azure"
    $armDeployment = New-AzureRmResourceGroupDeployment -Name $azureDeploymentName -ResourceGroupName $deploymentName `
          -TemplateFile ((Get-Item -Path ".\").FullName + "\" + $armTemplate) -TemplateParameterObject $parameters[$armTemplate]
    
    ## Collecting ARM deployment outputs
    foreach($outKey in $armDeployment.Outputs.Keys) 
    { 
        $outputs.Add($outKey, $armDeployment.Outputs[$outKey].Value)
    } 

    ## ARM deployment #2 (Function App and App Srv Plan)
    $idx += 1
    # inserting ARM deployment #1 outputs which will be used for function app site config into the arm template 2 parameters template 
    $paramsTemplate = (Get-Content .\arm\armtemplate2siteconfigtemplate.json -Raw)
    $paramsTemplate = $paramsTemplate.Replace("{storageaccountname}", $outputs["storageAccountName"])
    $paramsTemplate = $paramsTemplate.Replace("{storageaccountkey}", $outputs["storageAccountKey"])
    $paramsTemplate = $paramsTemplate.Replace("{sqldb}", $outputs["sqlDatabase"])
    $paramsTemplate = $paramsTemplate.Replace("{sqlserver}", $outputs["sqlServer"])
    $paramsTemplate = $paramsTemplate.Replace("{sqlserverusername}", $outputs["sqlServerUsername"])
    $paramsTemplate = $paramsTemplate.Replace("{sqlserverpassword}", $outputs["sqlServerPassword"])
    $paramsTemplate = $paramsTemplate.Replace("{watttimeapikey}", $wattTimeApiKey)
    $paramsTemplate = $paramsTemplate.Replace("{watttimeusername}", $wattTimeUsername)
    $paramsTemplate = $paramsTemplate.Replace("{watttimepassword}", $wattTimePassword)
    $paramsTemplate = $paramsTemplate.Replace("{watttimeemail}",   $wattTimeEmail)
    $paramsTemplate = $paramsTemplate.Replace("{watttimeorganization}",$wattTimeOrg)
    $paramsTemplate = $paramsTemplate.Replace("{darkskiapikey}", $darkSkyApiKey)

    # saving siteconfig to arm template parameters file to be used in arm deploment #2
    $paramsFile = ".\arm\armtemplate2siteconfig.json"
    $paramsTemplate | Out-File -FilePath $paramsFile

    $azureDeploymentName = $deploymentName + "-" + $idx
    $armTemplate = $armTemplates[$idx]
    Write-Output "Deploying via ARM template, $armTemplate, into Azure"
    $armDeployment = New-AzureRmResourceGroupDeployment -Name $azureDeploymentName -ResourceGroupName $deploymentName `
          -TemplateFile ((Get-Item -Path ".\").FullName + "\" + $armTemplate) -TemplateParameterFile $paramsFile

    ## Collecting ARM deployment outputs
    foreach($outKey in $armDeployment.Outputs.Keys) 
    { 
        $outputs.Add($outKey, $armDeployment.Outputs[$outKey].Value)
    } 
    
    # Uploading CIQS function helpers to Function App service
    Write-Output "Uploading CIQS function helpers to Function App Service....."
    UploadFunctionsToFunctionApp .\ciqsfunctions.zip  $outputs["functionAppName"]

    $functionsEndpoint = $outputs["functionAppBaseUrl"]
    # Set parameters (function name and POST body) for invoking the azure function
    $functionName = $functionPrepSqlName     
    $connString = $outputs["sqlConnectionString"]
    $postParams = @{sqlConnectionString=$connString}

    populateDatabase -ResourceGroupName $deploymentName
    
    # DEPLOYMENT COMPLETE
    Write-Output "DEPLOYMENT COMPLETED.`r`nPlease return to the Github Page for additional instructions. These output values will likely come in handy:`r`n`r`nOUTPUTS:" $outputs   
}

function populateDatabase(
    # Parameter help description
    [string]
    $ResourceGroupName
) {
    $servername = (Get-AzureRmSqlServer -ResourceGroupName $ResourceGroupName).ServerName
    $dbname = (Get-AzureRmSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $servername).DatabaseName.Where( {$_ -ne 'master'} )[0]
    $containerName = "$ResourceGroupName-blob"
    
    $username = 'abcdefg'
    $pwd = 'Passw0rd-2018' | ConvertTo-SecureString -AsPlainText -Force
    
    $storageAccount = Get-AzureRmStorageAccount -ResourceGroupName $ResourceGroupName
    $storageAccountName = $storageAccount.StorageAccountName
    $blobStorage = New-AzureStorageContainer -Name $containerName -Context $storageAccount.Context -Permission blob
    
    $filePath = ".\carbon_emissions_v1.bacpac"
    $outputFileName = "seed.bacpac"
    Write-Host "Uploading file: '$filePath' to blob storage '$blobStorage.' as filename '$outputFileName'"
    $file = ($blobStorage | Set-AzureStorageBlobContent -File $filePath -Blob $outputFileName)
    $storageUri = $file.ICloudBlob.Uri.AbsoluteUri
    
    Write-Host "Importing data from bacpac file '$filepath' into database '$ResourceGroupName/$dbname'"
    $importRequest = New-AzureRmSqlDatabaseImport `
        -ResourceGroupName $ResourceGroupName `
        -ServerName $servername `
        -DatabaseName $dbname `
        -StorageKeyType "StorageAccessKey" `
        -StorageKey $(Get-AzureRmStorageAccountKey -ResourceGroupName $ResourceGroupName -StorageAccountName $storageAccountName).Value[0] `
        -StorageUri $storageUri `
        -AdministratorLogin $username `
        -AdministratorLoginPassword $pwd `
        -Edition Standard `
        -ServiceObjectiveName S0 `
        -DatabaseMaxSizeBytes 5000000

    # Check import status and wait for the import to complete
    Write-Host "Waiting for import to complete"
    $importStatus = Get-AzureRmSqlDatabaseImportExportStatus -OperationStatusLink $importRequest.OperationStatusLink
    [Console]::Write("Importing")
    while ($importStatus.Status -eq "InProgress")
    {
        $importStatus = Get-AzureRmSqlDatabaseImportExportStatus -OperationStatusLink $importRequest.OperationStatusLink
        [Console]::Write(".")
        Start-Sleep -s 10
    }
    [Console]::WriteLine("")
    if ($importStatus.Status -eq "Succeeded") {
        Write-Host "Import has succeeded" -ForegroundColor "Green"
    } else {
        Write-Host "Import has not succeeded" -ForegroundColor "Red"
    }
    
}