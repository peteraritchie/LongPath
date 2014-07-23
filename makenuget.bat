@REM TODO: if not exist nuget\lib\net40-client md nuget\lib\net40-client
@REM TODO: copy src\geschikt\bin\Release\geschikt.dll nuget\lib\net40-client
if not exist nuget\lib\net45 md nuget\lib\net45
copy src\Pri.LongPath\bin\Release\gPri.LongPath.dll nuget\lib\net45
@REM TODO: if not exist nuget\lib\sl4-wp md nuget\lib\sl4-wp 
@REM TODO: copy src\geschikt.wp7\Bin\Release\geschikt.wp7.dll nuget\lib\sl4-wp 

pushd nuget
..\util\nuget.exe pack LongPath.nuspec 
popd
