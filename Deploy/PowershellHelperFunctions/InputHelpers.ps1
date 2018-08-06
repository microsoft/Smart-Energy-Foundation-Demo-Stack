function GetValidInput($inputKeyword, $allowedParamInput)
{
    $input = "******"
    while($allowedParamInput -notcontains $input)
    { 
        if($input -ne "******")
        {
            Write-Host ("`r`nPlease enter a valid " + $inputKeyword + " from the following supported " + $inputKeyword + "s list.`r`n")
        }
        $input = Read-Host -Prompt ("Please enter a " + $inputKeyword + " for the deployment. The likely supported " + $inputKeyword + "s are based on your subscription are: `r`n`r`n" + ($allowedParamInput -join ', ')) 
    }

    return $input
}

function GetValidInputWithRegex($inputKeyword, $inputRegex)
{
    $input = "******"
    while($input -notmatch $inputRegex)
    { 
        if($input -ne "******")
        {
            Write-Host ("`r`nThe input was invalid. Please re-enter it.`r`n")
        }
        $input = Read-Host -Prompt ("Please enter a " + $inputKeyword) 
    }

    return $input
}