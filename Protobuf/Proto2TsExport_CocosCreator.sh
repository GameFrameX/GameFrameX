#!/bin/bash

dotnet ./Tools/ProtoExport.dll  \
    --mode typescript \
    --inputPath ./ \
    --outputPath ../CocosCreator/extensions/com.gameframex.framework/runtime/gameframex/protobuf \
    --isGenerateErrorCode true \
    --importPath "../network/"

echo "TypeScript 导出完成: CocosCreator/assets/gameframex/protobuf/"
