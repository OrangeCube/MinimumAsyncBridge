pushd ..\src\MinimumAsyncBridge.Nuget
.\pack.bat
popd

if (-not (Test-Path ../../packages)) { mkdir ../../packages }
cp ..\src\MinimumAsyncBridge.Nuget\*.nupkg ../../packages
