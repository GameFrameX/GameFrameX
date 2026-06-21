#!/bin/bash

# 切换到工具输出目录（处理符号链接）
cd -P ../Tools/ProtoExport/bin/Debug/net10.0

# 导出 TypeScript 到 LayaBox 项目
dotnet ProtoExport.dll \
    --mode typescript \
    --inputPath ./../../../../../Protobuf \
    --outputPath ./../../../../../LayaBox/assets/scripts/protobuf \
    --isGenerateErrorCode true \
    --importPath "../../../src/gameframex/network/"

echo "TypeScript 导出完成: LayaBox/assets/scripts/protobuf/"
