cd /d ../Tools/ProtoExport\bin\Debug\net10.0
call dotnet ProtoExport.dll --mode csharp --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" --inputPath ./../../../../../Protobuf --outputPath ./../../../../../Unity/Assets/Hotfix/Proto --namespaceName Hotfix.Proto --isGenerateErrorCode true

pause
