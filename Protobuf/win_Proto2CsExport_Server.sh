#!/bin/bash

# 切换目录，-P 选项是用来处理符号链接的
cd -P ../Proto2CSApp/bin/Debug/net6.0

# 启动应用程序，并将其放入后台执行
./Proto2CSApp.exe "../../../../Protobuf" "client" "../../../../Server/GameFrameX.Proto/Proto" "Server.Proto.Proto" &

# 输出提示信息
echo "Press any key to continue . . ."

# 等待用户按下任意键
read -n 1 -s -r -p ""