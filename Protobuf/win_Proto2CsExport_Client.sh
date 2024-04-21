#!/bin/bash

# 切换目录，-P 选项是用来处理符号链接的
cd -P ../Proto2CSApp/bin/Debug/net6.0

# 启动应用程序，& 表示在后台运行
./Proto2CSApp.exe "../../../../Protobuf" "client" "../../../../Server/GameFrameX.Proto/Proto" "Server.Proto.Proto" &

# 暂停，等待用户按任意键继续，由于 shell 环境没有直接的 pause 命令，使用 read 模拟
read -p "Press any key to continue . . . " -n1 -s

echo ""  # 输出一个新行