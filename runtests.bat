@choice /m "This will crash Visual Studio with Update 4 and prior, press Y to continue or N to abort"
@if %errorlevel%==2 goto:eof
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\..\ide\commonextensions\microsoft\testwindow\vstest.console.exe" Tests\bin\Debug\Tests.dll 
call cleartestresults.bat 
