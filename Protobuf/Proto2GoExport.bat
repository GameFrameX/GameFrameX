cd /d ../Tools/ProtoExport\bin\Debug\net8.0
call dotnet ProtoExport.dll --mode go --inputPath ./../../../../../Protobuf --outputPath ./../../../../../GoServer/proto --namespaceName proto --isGenerateErrorCode true

pause
