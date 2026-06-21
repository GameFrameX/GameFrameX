#!/bin/bash

# 切换目录，-P 选项是用来处理符号链接的
cd -P ../Tools/ProtoExport/bin/Debug/net8.0

# 启动应用程序，并将其放入后台执行
dotnet ProtoExport.dll --mode cpp --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" --inputPath ./../../../../../Protobuf --outputPath ./../../../../../Unreal/Source/Proto --namespaceName GameFrameX.Proto --isGenerateErrorCode true &

# 输出提示信息
echo "Press any key to continue . . ."

# 等待用户按下任意键
read -n 1 -s -r -p ""
