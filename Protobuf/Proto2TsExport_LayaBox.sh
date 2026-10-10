#!/bin/bash

dotnet ./Tools/ProtoExport.dll  \
    --mode typescript \
    --inputPath ./ \
    --outputPath ../LayaBox/src/gameframex/protobuf \
    --isGenerateErrorCode true \
    --importPath "../network/"

echo "TypeScript 导出完成: LayaBox/src/gameframex/protobuf/"
