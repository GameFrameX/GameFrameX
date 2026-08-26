dotnet ./Tools/ProtoExport.dll --mode csharp --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" --inputPath ./ --outputPath ../Unity/Assets/Hotfix/Proto --namespaceName Hotfix.Proto --isGenerateErrorCode true
pause
