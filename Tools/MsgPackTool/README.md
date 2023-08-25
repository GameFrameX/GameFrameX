# MsgPackTool

协议类文件的生成`MessagePack` 的`Formatter`工具
> Clone 自 Geek.MsgPackTool

## 仓库地址

https://github.com/leeveel/Geek.MsgPackTool

## 版本记录

https://github.com/leeveel/Geek.MsgPackTool/commits/e77e14c76b13e7871bf641a2885031f0aa934e2c

# 配置文件

文件目录`MessagePack.Generator/Configs/config.json`

## 参数解析

```json5

{
  "ServerProtoPath": "../../../Server/Server.Proto",
  // 服务器的协议类文件放置目录
  "ServerNameSpace": "",
  // 服务器生成的命名空间
  "ServerOutPath": "../../../Server/Server.Proto/Formatter",
  // 服务器的Formatter 输出目录
  "ServerIsGenerated": false,
  // 是否生成服务器的Formatter 文件
  "ClientProtoPath": "../../../Unity/Assets/Hotfix.Proto",
  // 客户端的协议类文件放置目录
  "ClientFormatNameSpace": "",
  // 客户端生成的命名空间
  "ClientOutPath": "../../../Unity/Assets/Hotfix.Proto/Generated",
  // 客户端的Formatter 输出目录
  "BaseMessageName": "Message",
  // 协议文件的基类
  "NoExportList": [
    // 导出忽略类
    "ProtoMessageIdHandler",
    "HotfixProtoHandler"
  ]
}


```