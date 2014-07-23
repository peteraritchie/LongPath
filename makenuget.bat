@REM TODO: if not exist nuget\lib\net40-client md nuget\lib\net40-client
@REM TODO: copy Pri.LongPath\bin\Release\Pri.LongPath.dll nuget\lib\net40-client
if not exist nuget\lib\net45 md nuget\lib\net45
copy Pri.LongPath\bin\Release\Pri.LongPath.dll nuget\lib\net45

pushd nuget
..\util\nuget.exe pack LongPath.nuspec 
popd
