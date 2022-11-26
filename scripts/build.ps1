cd "$PSScriptRoot/.."

$configuration = "Release"
if ($args[0]) {
    $configuration = $args[0]
}

$path = "build-$configuration"
if ($args[1]) {
    $path = $args[1]
}

$majorVer = Get-Content VERSION
$lastVer = git tag --sort version:refname --list "$majorVer.*" | Select -last 1

echo "Configuration: $configuration"
echo "BuildPath: $path"
echo "Major: $majorVer"
echo "lastVer: $lastVer"

if ($lastVer -ne "") {
    $nextVer = $lastVer.Split('.')
    $lastItem = $nextVer.Length-1
    $nextVer[$lastItem] = [int]$nextVer[$lastItem] + 1
    $nextVer = $nextVer -Join '.'
} else {
    $nextVer="$majorVer.0"
}

dotnet --list-sdks -or winget install Microsoft.DotNet.SDK.6
dotnet build --configuration "$configuration" "/property:Version=$nextVer" --output "$path"
