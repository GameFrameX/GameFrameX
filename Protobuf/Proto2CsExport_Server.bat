cd /d ../Tools/ProtoExport\bin\Debug\net8.0
start dotnet ProtoExport.dll --mode server --inputPath ./../../../../../Protobuf --outputPath ./../../../../../Server/GameFrameX.Proto/Proto --namespaceName GameFrameX.Proto.Proto --isGenerateErrorCode true

pause