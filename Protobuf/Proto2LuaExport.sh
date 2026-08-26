#!/bin/bash

dotnet ./Tools/ProtoExport.dll  --mode lua --importPath "./network/" --inputPath ./ --outputPath ../Defold/scripts/protobuf --isGenerateErrorCode true
