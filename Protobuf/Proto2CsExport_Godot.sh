#!/bin/bash

dotnet run --project ../Tools/ProtoExport/ProtoExport.csproj -- --mode csharp --usingStatements "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime" --inputPath ./ --outputPath ../Godot/Assets/Hotfix/Proto --namespaceName Hotfix.Proto --isGenerateErrorCode true
