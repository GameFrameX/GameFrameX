GameFrameX是一款全面的集成式游戏开发框架，提供了从前端到后端的完整解决方案。该框架支持包括Unity、Cocos Creator、LayaBox、和Godot等多种主流游戏开发平台，确保开发者能够为不同的目标平台打造精美的游戏体验。

此外，GameFrameX拥有多进程服务器的支持，并且集成了Docker的自动化构建和部署，大大简化了游戏发布和维护过程。无论是在客户端管理、运维管理、数据管理，还是游戏数据的具体运营方面，GameFrameX都提供了强有力的后台支持，使得监控、优化和更新游戏变得前所未有的简单。

游戏团队通过GameFrameX能够实现资源的高效分配和管理，加快开发周期，降低运营成本，并且最终增强玩家的游戏体验。整合了先进的技术和易用的界面，GameFrameX为游戏开发和运营的各个环节提供了高效、灵活且可扩展的解决方案，让游戏从构想到上线，再到生命周期管理都成为一件轻松愉快的事情。

GameFrameX不仅是一套综合性的游戏开发与运维框架，它也将数据管理和流程自动化提升至新的高度。该框架旨在通过使用MongoDB作为游戏数据库，提供了高性能、灵活的数据存储方案，这确保了大规模和复杂数据的快速读写能力，极大地增强了游戏的扩展性和稳定性。

在数据表的导入方面，GameFrameX采用了LuBan工具，它自动化并优化了数据表的处理流程，极大提高了开发效率，并且保证了数据的准确性和一致性。

为了满足后台管理的需求，GameFrameX选择了MySQL数据库，提供了一个稳固和可靠的后端管理系统。结合MongoDB和MySQL，GameFrameX在数据解决方案上实现了多样性和功能性的平衡，以满足不同场景下的业务需要。

关于自动化构建和部署，GameFrameX通过支持Docker技术，让应用的打包、分发变得更加高效和标准化，确保了在不同环境下软件能够以同样的方式运行。此外，GameFrameX还整合了Codeup代码托管平台的自动构建系统，允许通过`tag`
标签触发构建过程，进一步实现了代码变更的连贯性管理及自动化部署，减少了人工干预，提升了运维效率。

GameFrameX为游戏开发者提供了一个强大的技术生态，将数据库管理、后台服务、自动化构建和部署融于一体，使游戏的开发、运营和维护实现流水线式高效协作，为开发者提供了无与伦比的便捷与保障。

> 以上介绍通过GPT4生成

# 开发进度：

请关注 Projects

## 开发内容在 `develop` 分支

> 正在开发中...

# 服务器（从 `geekserver` 修改而来.）

    https://github.com/AlianBlank/GameFrameX.Server

# 管理后台

    https://github.com/AlianBlank/GameFrameX.Admin

# 工具集

    https://github.com/AlianBlank/GameFrameX.Tools

# 客户端

## Unity

    https://github.com/AlianBlank/GameFrameX.Unity

## Laya Box

    https://github.com/AlianBlank/GameFrameX.LayaBox

## Cocos Creator

    https://github.com/AlianBlank/GameFrameX.CocosCreator

## Godot

    https://github.com/AlianBlank/GameFrameX.Godot

目录结构要求：由于项目基本上都是使用相对目录的方式。请不要乱放文件夹

```
GameFrameX/ 项目根目录.可以根据自己的项目修改
├── Config/  # 配置表放置目录。使用的`LuBan` 导表方案 https://github.com/focus-creative-games/luban
│ ├── Defines/ # LuBan 的常量定义配置文件目录
│ ├── Excels/ # 核心配置表文件夹。所有的Excel配置文件存放目录
│ └── luban.conf/ # Luban 配置文件。如需修改。请查阅LuBan文档.链接：https://github.com/focus-creative-games/luban
├── docker/  # Docker本地运行目录
├── Docs/ # 文档相关内容。目前是GeekServer的原始文档
├── FairyGUIProject/ # FairyGUI 项目目录。如果不需要可删除。
├── Protobuf/ # 前后端或后端之前的通讯协议定义文件。采用ProtoBuf 描述文件。
├── Server/ # 游戏服务器解决方案文件夹。请从  https://github.com/AlianBlank/GameFrameX.Server  Clone 到此目录下
│ ├── Server.XXX # 服务器文件夹开始
│ ├── ... # 服务器解决方案文件夹列表
│ └── Server.XXX # 服务器文件夹结束
├── Laya/ # Laya客户端文件夹。请从  https://github.com/AlianBlank/GameFrameX.LayaBox  Clone 到此目录下
├── CocosCreator/ # CocosCreator客户端文件夹。请从  https://github.com/AlianBlank/GameFrameX.CocosCreator  Clone 到此目录下
├── Godot/ # Godot客户端文件夹。请从  https://github.com/AlianBlank/GameFrameX.Godot  Clone 到此目录下
├── Unity/ # Unity客户端文件夹。请从  https://github.com/AlianBlank/GameFrameX.Unity  Clone 到此目录下
│ ├── Assets/ # Unity客户端文件夹
│ ├── Packages/ # Unity客户端文件夹
│ ├── ProjectSettings/ # Unity项目设置文件夹
│ └── UserSettings/ # Unity项目用户设置文件夹
└── LICENSE # 许可证文件
```

# 交流方式(建议。需求。BUG)

<!-- <div  align="center">    

<img src="images/wechat_group.png" width = "226" height = "290" alt=""/>

<img src="images/qq_group.png" width = "226" height = "290" alt=""/>

</div> -->

QQ群：467608841

# Doc (没空写-.-)

文档地址 : https://www.yuque.com/alianblank/gameframex

# 贡献名单

## Tools

[bambom](https://github.com/bambom)

## FairyGUI

[PlayerYF](https://github.com/PlayerYF)

# 免责声明

所有插件均来自互联网.请各位使用时自行付费.如果以上插件涉及侵权.请发email.本人将移除.谢谢

# LICENSE

```
MIT License

Copyright (c) 2023 Blank

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
