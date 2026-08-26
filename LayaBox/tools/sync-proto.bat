@echo off
set "sourceDir=.\..\..\Protobuf"
set "destDir=..\assets\resources\protobuf"

if not exist "%destDir%" (
    echo Creating directory: "%destDir%"
    mkdir "%destDir%"   
)

echo sync:  "%sourceDir%  To %destDir%"

for /d %%f in (%sourceDir%*) do (
    if not exist %destDir%%%~nf xcopy %sourceDir% %destDir% /Y /E /S /V /I /T 
)

pause