if not exist nuget\lib\net40 md nuget\lib\net40
copy Pri.LongPath.net40\bin\Release\Pri.LongPath.dll nuget\lib\net40
if not exist nuget\lib\net20 md nuget\lib\net20
copy Pri.LongPath.net20\bin\Release\Pri.LongPath.dll nuget\lib\net20
if not exist nuget\lib\net45 md nuget\lib\net45
copy Pri.LongPath\bin\Release\Pri.LongPath.dll nuget\lib\net45

pushd nuget
..\util\nuget.exe pack LongPath.nuspec 
popd
