pushd ..\src\nuspecs
.\pack.bat
popd

if (-not (Test-Path ../../packages)) { mkdir ../../packages }
cp ..\src\nuspecs\*.nupkg ../../packages
