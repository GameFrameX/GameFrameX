cd /d ../Tools/ProtoExport\bin\Debug\net10.0
call dotnet ProtoExport.dll --mode csharp --isServer true --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" --isGenerateDescription true --inputPath ./../../../../../Protobuf --outputPath ./../../../../../Server/GameFrameX.Proto/Proto --namespaceName GameFrameX.Proto.Proto --isGenerateErrorCode true

pause
