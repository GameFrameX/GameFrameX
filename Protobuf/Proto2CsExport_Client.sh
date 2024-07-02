#!/bin/bash

# 切换目录，-P 选项是用来处理符号链接的
cd -P ../Tools/ProtoExport/bin/Debug/net8.0

# 启动应用程序，& 表示在后台运行
dotnet ProtoExport.dll --mode unity --inputPath ./../../../../../Protobuf --outputPath ./../../../../../Unity/Assets/Hotfix/Proto --namespaceName Hotfix.Proto --isGenerateErrorCode true &

# 暂停，等待用户按任意键继续，由于 shell 环境没有直接的 pause 命令，使用 read 模拟
read -p "Press any key to continue . . . " -n1 -s

echo ""  # 输出一个新行