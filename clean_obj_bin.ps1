try 
{
    $folders = Get-ChildItem -Directory -Recurse | Where-Object { $_.Name -eq "bin" -or $_.Name -eq "obj" }

    if (!$folders) 
    {
        Write-Output "No bin or obj folders found."
    } 
    else 
    {
        foreach ($folder in $folders) 
        {
            $folderFullName = $folder.FullName;
            Write-Output "Deleting folder: $($folderFullName)"
            Remove-Item $folderFullName -Force -Recurse
        }
    }
}
catch 
{
    Write-Host $Error[0].Exception.GetType().FullName;
}

Write-Host "Press any key to continue..."
[System.Console]::ReadKey('NoEcho,IncludeKeyDown')