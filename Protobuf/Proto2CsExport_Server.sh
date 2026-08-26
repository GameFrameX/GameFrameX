#!/bin/bash

dotnet ./Tools/ProtoExport.dll  --mode csharp --isServer true --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages" --isGenerateDescription true --inputPath ./ --outputPath ../Server/GameFrameX.Proto/Proto --namespaceName GameFrameX.Proto.Proto --isGenerateErrorCode true
