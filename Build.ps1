param([String]$cfg='Release')

Set-Location $PSScriptRoot

$target = "Industrial.Protocols.HART"

$path = "$target/bin/$cfg"

[array]$files = Get-ChildItem "$path/*.nupkg"
$file = $files[$files.Length - 1]

Compress-Archive -Path "$path/lib" -DestinationPath $file -Update
Copy-Item -Path $file -Destination "Bin" -Recurse -Force

dotnet nuget push $file -k $env:nk -s https://api.nuget.org/v3/index.json --skip-duplicate

Remove-Item "$path/lib" -Recurse -Force
Remove-Item $file -Force