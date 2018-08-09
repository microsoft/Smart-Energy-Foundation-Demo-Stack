function ConvertTo-FileUri {
    param (
        [Parameter(Mandatory)]
        [string]
        $Path
    )

    $SanitizedPath = $Path -replace "\\", "/" -replace " ", "%20"
    "file:{0}" -f $SanitizedPath
}


function Zip($source, $filename)
{
    Compress-Archive -Path $source -DestinationPath $filename -CompressionLevel Optimal -Force
    return $filename
}