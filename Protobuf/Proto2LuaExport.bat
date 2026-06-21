cd /d ../Tools/ProtoExport\bin\Debug\net8.0
call dotnet ProtoExport.dll --mode lua --importPath "./network/" --inputPath ./../../../../../Protobuf --outputPath ./../../../../../Defold/scripts/protobuf --isGenerateErrorCode true

pause
