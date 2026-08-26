#!/bin/bash

dotnet ./Tools/ProtoExport.dll  \
    --mode typescript \
    --inputPath ./ \
    --outputPath ../LayaBox/assets/scripts/protobuf \
    --isGenerateErrorCode true \
    --importPath "../../../src/gameframex/network/"

echo "TypeScript 导出完成: LayaBox/assets/scripts/protobuf/"
