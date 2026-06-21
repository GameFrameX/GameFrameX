cd /d ../Tools/ProtoExport\bin\Debug\net8.0
call dotnet ProtoExport.dll --mode typescript --inputPath ./../../../../../Protobuf --outputPath ./../../../../../Laya/src/gameframex/protobuf --isGenerateErrorCode true

pause
