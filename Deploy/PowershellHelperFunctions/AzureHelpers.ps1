function GetResourceTypesForTemplate($armTemplates)
{
    $resourceTypesSet = New-Object System.Collections.Generic.HashSet[string]
    foreach($template in $armTemplates)
    {

        $json = Get-Content -Path ((Get-Item -Path ".\").FullName + "\" + $template) | ConvertFrom-Json
        $rTypes = $json.resources;
        if($rTypes.Count -eq 0)
        {
            $resourceTypesSet.Add("microsoft.storage/storageaccounts")
        }


        foreach($res in $rTypes)
        {
            $tp = $res.type.ToLowerInvariant()
            if($tp -ne "microsoft.datafactory/datafactories")
            {
                $resourceTypesSet.Add($tp) | Out-Null
            }
        }
    }

    return $resourceTypesSet
}

function GetLikelyLocationsForTemplate($templateResourceTypesSet)
{
    $supportedLocationsByResourceTypes = @{}

    Foreach($resourceType in $templateResourceTypesSet)
    {
        $resNamespace = $resourceType.Split("/")[0]
        $resName = $resourceType.Split("/")[1]

        $locs = ((Get-AzureRmResourceProvider -ProviderNamespace $resNamespace).ResourceTypes | Where-Object ResourceTypeName -eq $resName).Locations

        # dictionary has resourceType as key and the list of supported locations for that resourceType
        $supportedLocationsByResourceTypes.Add($resourceType, $locs)
    }

    if($supportedLocationsByResourceTypes.Count -eq 0)
    {
        return @()
    }

    $first = $templateResourceTypesSet[0]
    $result = $supportedLocationsByResourceTypes[$first]
    
    foreach ($kvp in $supportedLocationsByResourceTypes.GetEnumerator()) {
        $key = $kvp.Key
        $loc = $kvp.Value

        # Intersect the supported locations for the various resourceTypes
        $result = Compare-Object $result $loc -PassThru -IncludeEqual -ExcludeDifferent
    }

    return $result
}

function Get-AccessToken($tenantId) {
    $cache = [Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache]::DefaultShared
    $cacheItem = $cache.ReadItems() | Where {$_.TenantId -eq $tenantId} | Select-Object -First 1
    if ($cacheItem -eq $null) {
        $cacheItem = $cache.ReadItems() | Select-Object -First 1
    }
    
    # when running in VSTS Build Agent the $cacheItem should contain an AccessToken
    if (-not [string]::IsNullOrEmpty($cacheItem.AccessToken))
    {
        return $cacheItem.AccessToken;
    }
    # when running locally $cacheItem will not contain an AccessToken
    else
    {
        if(-not (Get-Module AzureRm.Profile)) {
            Import-Module AzureRm.Profile
        }
        $azureRmProfileModuleVersion = (Get-Module AzureRm.Profile).Version
        # refactoring performed in AzureRm.Profile v3.0 or later
        if($azureRmProfileModuleVersion.Major -ge 3) {
            $azureRmProfile = [Microsoft.Azure.Commands.Common.Authentication.Abstractions.AzureRmProfileProvider]::Instance.Profile
            if(-not $azureRmProfile.Accounts.Count) {
            Write-Error "Ensure you have logged in before calling this function."    
            }
        } else {
            # AzureRm.Profile < v3.0
            $azureRmProfile = [Microsoft.WindowsAzure.Commands.Common.AzureRmProfileProvider]::Instance.Profile
            if(-not $azureRmProfile.Context.Account.Count) {
            Write-Error "Ensure you have logged in before calling this function."    
            }
        }
    
        $currentAzureContext = Get-AzureRmContext
        $profileClient = New-Object Microsoft.Azure.Commands.ResourceManager.Common.RMProfileClient($azureRmProfile)
        Write-Debug ("Getting access token for tenant" + $currentAzureContext.Subscription.TenantId)
        $tokenAD = $profileClient.AcquireAccessToken($currentAzureContext.Subscription.TenantId)
        return $tokenAD.AccessToken
    }
}

function SelectSubscription()
{
    $subscriptionId = Get-SubscriptionId
    $subscriptionName = Get-SubscriptionName

    $input = Read-Host -Prompt ("Currently selection subscription is: Name=$subscriptionName; Id=$subscriptionId. If you'd like to change the selected subscription enter 0. Otherwise simply hit enter to proceed with the currently selected subscription")
    if($input -eq "0")
    {
        $subsObj = Get-AzureRmSubscription
        $subIds = $subsObj | Foreach {"$($_.Id)"} 
        $subNames = $subsObj | Foreach {"$($_.Name)"} 

        if($subIds.Count -eq 1)
        {
            Write-Output "$subscriptionName : is the only available subscription for your account. The deployment will be created in this account"
            return
        }

        $selectedSubId = "******"
        while($subIds -notcontains $selectedSubId)
        { 
            if($selectedSubId -ne "******")
            {
                Write-Host ("`r`nInvalid ID entered. Please enter a valid subscription ID from the following subscriptions available for your account: ")
            }
            Write-Output "Please enter a valid subscription ID from the following subscriptions available for your account:`r`n"
            for($idx = 0; $idx -lt $subIds.Count; $idx++)
            {
                Write-Output "Id:  $($subIds[$idx])  (Name: $($subNames[$idx]))"
            }
            $selectedSubId = Read-Host -Prompt ("Subscription ID:   ")
        }

        if($selectedSubId -eq $subscriptionId)
        {
            Write-Output "The unchanged selected subscription is:  $subscriptionName (Id: $subscriptionId)"
            return $null
        }

        Get-AzureRmSubscription -SubscriptionId $selectedSubId | Select-AzureRmSubscription 
        $subscriptionId = $selectedSubId
        $idx = $subIds.IndexOf($subscriptionId)
        $subscriptionName = $subNames[$idx]
        Write-Output "The newly selected subscription is $subscriptionName (Id: $subscriptionId)"
    }
}

function Get-SubscriptionId()
{
    $currentAzureContext = Get-AzureRmContext
    if($currentAzureContext -ne $null)
    {
        return $currentAzureContext.Subscription.Id
    }

    return $null
}

function Get-TenantId()
{
    $currentAzureContext = Get-AzureRmContext
    if($currentAzureContext -ne $null)
    {
        return $currentAzureContext.Tenant.Id
    }

    return $null
}

function Get-SubscriptionName()
{
    $currentAzureContext = Get-AzureRmContext
    if($currentAzureContext -ne $null)
    {
        return $currentAzureContext.Subscription.Name
    }

    return $null
}

function UploadFunctionsToFunctionApp($zipFilePath, $appName)
{
    $apiUrl = "https://" + $appName + ".scm.azurewebsites.net/api/zip/site/wwwroot"
    $userAgent = "powershell/2.0"
    $tenantId = (Get-AzureRmContext).Tenant.TenantId
    $subscription = (Get-AzureRmContext).Subscription.SubscriptionId
    $token = Get-AccessToken $tenantId
    Invoke-RestMethod -Uri $apiUrl -Headers @{Authorization=("Bearer {0}" -f $token)} -UserAgent $userAgent -Method PUT -InFile $zipFilePath -ContentType "multipart/form-data"
}