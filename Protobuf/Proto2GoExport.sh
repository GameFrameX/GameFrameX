#!/bin/bash

dotnet ./Tools/ProtoExport.dll --mode go --inputPath ./ --outputPath ../GoServer/proto --namespaceName proto --isGenerateErrorCode true
