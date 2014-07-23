@echo off
if not exist TestResults goto :EOF
setlocal
set folder=TestResults
set MT="%TEMP%\DelFolder_%RANDOM%"
MD %MT%
RoboCopy %MT% %folder% /MIR
RD /S /Q %MT%
RD /S /Q %folder%
endlocal