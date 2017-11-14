# If Program Files (x86) is found, use Program Files (x86). If Program Files (x86) is not found it, use Program Files
if (Test-Path ${env:ProgramFiles(x86)})
{
    $Script:ProgramFiles = ${env:ProgramFiles(x86)}
}
else
{
    $Script:ProgramFiles = $env:ProgramFiles
}


# Candidates. Search from the top, return the first found
$Script:items =
    # VS 2017
    '\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe',
    '\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe',
    '\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe',
    '\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe',
    # VS 2015
    '\MSBuild\14.0\Bin\MSBuild.exe',
    # VS 2013
    '\MSBuild\12.0\Bin\MSBuild.exe'


foreach ($Script:item in $Script:items)
{
    $Script:msbuild = $Script:ProgramFiles + $Script:item
    if (Test-Path $Script:msbuild) { return $Script:msbuild }
}