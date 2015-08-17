if (Test-Path ${env:ProgramFiles(x86)})
{
    $Script:ProgramFiles = ${env:ProgramFiles(x86)}
}
else
{
    $Script:ProgramFiles = $env:ProgramFiles
}

if (Test-Path $($Script:ProgramFiles + '\MSBuild\14.0\Bin\MSBuild.exe'))
{
    $Script:msbuild = $Script:ProgramFiles + '\MSBuild\14.0\Bin\MSBuild.exe'
}
else
{
    $Script:msbuild = $Script:ProgramFiles + '\MSBuild\12.0\Bin\MSBuild.exe'
}

$Script:msbuild
