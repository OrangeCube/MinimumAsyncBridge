param([hashtable] $set)

if ($set -eq $null)
{
    $set = @{}
}

if ($Script:msbuild -eq $null)
{
    $Script:msbuild = .\Get-VsPath.ps1
}

function build
{
    process
    {
        $Local:sln = $_

        ./NuGet.exe restore $Local:sln

        . $Script:msbuild $Local:sln /t:Build
    }
}

function build-dependecy
{
    process
    {
        if ($set.ContainsKey($_))
        {
            return;
        }

        $set.Add($_, $false)

        try
        {
            pushd $('..\..\' + $_ + '\build')
            & .\build.ps1 $set
        }
        finally
        {
            popd
        }
    }
}

if (Test-Path dependency.txt)
{
    cat ./dependency.txt | build-dependecy
}

cat ./solutions.txt | build

if (Test-Path postbuild.ps1)
{
    . ./postbuild.ps1
}
