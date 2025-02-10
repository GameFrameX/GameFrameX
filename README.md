<div align="center">
    <a href="https://trendshift.io/repositories/7536" target="_blank"><img src="https://trendshift.io/api/badge/repositories/7536" alt="GameFrameX%2FGameFrameX | Trendshift" style="width: 250px; height: 55px;" width="250" height="55"/></a>
</div>

GameFrameX是一款全面的集成式游戏开发框架，提供了从前端到后端的完整解决方案。该框架支持包括Unity、Cocos Creator、LayaBox、和Godot等多种主流游戏开发平台，确保开发者能够为不同的目标平台打造精美的游戏体验。

此外，GameFrameX拥有多进程服务器的支持，并且集成了Docker的自动化构建和部署，大大简化了游戏发布和维护过程。无论是在客户端管理、运维管理、数据管理，还是游戏数据的具体运营方面，GameFrameX都提供了强有力的后台支持，使得监控、优化和更新游戏变得前所未有的简单。

游戏团队通过GameFrameX能够实现资源的高效分配和管理，加快开发周期，降低运营成本，并且最终增强玩家的游戏体验。整合了先进的技术和易用的界面，GameFrameX为游戏开发和运营的各个环节提供了高效、灵活且可扩展的解决方案，让游戏从构想到上线，再到生命周期管理都成为一件轻松愉快的事情。

GameFrameX不仅是一套综合性的游戏开发与运维框架，它也将数据管理和流程自动化提升至新的高度。该框架旨在通过使用MongoDB作为游戏数据库，提供了高性能、灵活的数据存储方案，这确保了大规模和复杂数据的快速读写能力，极大地增强了游戏的扩展性和稳定性。

在数据表的导入方面，GameFrameX采用了LuBan工具，它自动化并优化了数据表的处理流程，极大提高了开发效率，并且保证了数据的准确性和一致性。

为了满足后台管理的需求，GameFrameX选择了MySQL数据库，提供了一个稳固和可靠的后端管理系统。结合MongoDB和MySQL，GameFrameX在数据解决方案上实现了多样性和功能性的平衡，以满足不同场景下的业务需要。

关于自动化构建和部署，GameFrameX通过支持Docker技术，让应用的打包、分发变得更加高效和标准化，确保了在不同环境下软件能够以同样的方式运行。此外，GameFrameX还整合了Codeup代码托管平台的自动构建系统，允许通过`tag`
标签触发构建过程，进一步实现了代码变更的连贯性管理及自动化部署，减少了人工干预，提升了运维效率。

GameFrameX为游戏开发者提供了一个强大的技术生态，将数据库管理、后台服务、自动化构建和部署融于一体，使游戏的开发、运营和维护实现流水线式高效协作，为开发者提供了无与伦比的便捷与保障。


# 服务器（从 `geekserver` 修改而来.）

    https://github.com/GameFrameX/GameFrameX.Server

# 管理后台(部分源码已不开放)

    https://github.com/GameFrameX/GameFrameX.Admin

[`演示站点 https://game.admin.web.vue.alianblank.com`](https://game.admin.web.vue.alianblank.com/)

# 工具集

    https://github.com/GameFrameX/GameFrameX.Tools

# 客户端

## Unity

    https://github.com/GameFrameX/GameFrameX.Unity

## Laya Box

    https://github.com/GameFrameX/GameFrameX.LayaBox

## Cocos Creator

    https://github.com/GameFrameX/GameFrameX.CocosCreator

## Godot

    https://github.com/GameFrameX/GameFrameX.Godot

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
├── Laya/ # Laya客户端文件夹。请从  https://github.com/GameFrameX/GameFrameX.LayaBox  Clone 到此目录下
├── CocosCreator/ # CocosCreator客户端文件夹。请从  https://github.com/GameFrameX/GameFrameX.CocosCreator  Clone 到此目录下
├── Godot/ # Godot客户端文件夹。请从  https://github.com/GameFrameX/GameFrameX.Godot  Clone 到此目录下
├── Unity/ # Unity客户端文件夹。请从  https://github.com/GameFrameX/GameFrameX.Unity  Clone 到此目录下
│ ├── Assets/ # Unity客户端文件夹
│ ├── Packages/ # Unity客户端文件夹
│ ├── ProjectSettings/ # Unity项目设置文件夹
│ └── UserSettings/ # Unity项目用户设置文件夹
└── LICENSE # 许可证文件
```

# 开始使用

1. 创建本地项目文件夹
2. 开启`cmd`或`shell`,cd 到本地存放目录
3. 执行以下命令

    ```shell
    git clone https://github.com/GameFrameX/GameFrameX.git
    git clone https://github.com/GameFrameX/GameFrameX.Server.git ./GameFrameX/Server
    git clone https://github.com/GameFrameX/GameFrameX.Tools.git ./GameFrameX/Tools
    git clone https://github.com/GameFrameX/GameFrameX.Unity.git ./GameFrameX/Unity
    
    ```

4. 打开`Tools`项目,编译一下。他目前是用来导出协议的
5. 打开`Unity`项目和`Server`直接启动即可食用。

# 交流方式(建议。需求。BUG)

<!-- <div  align="center">    

<img src="images/wechat_group.png" width = "226" height = "290" alt=""/>

<img src="images/qq_group.png" width = "226" height = "290" alt=""/>

</div> -->

QQ群：467608841

# Doc (已经在写了,别催了!-_-!)

`所有站点内容一致，不存在内容不一致的情况`

文档地址 : https://gameframex.doc.alianblank.com

备用文档地址 : https://gameframex-docs.pages.dev

备用文档地址 : https://gameframex.doc.cloudflare.alianblank.com

备用文档地址 : https://gameframex.doc.vercel.alianblank.com

# 赞赏一下呗

![wechat.jpg](Docs/imgs/wechat.jpg)

# 使用案例

| 游戏名称   | 链接地址                                       | 上线时间       |
|:-------|:-------------------------------------------|------------|
| 深夜的烧烤店 | [TapTap](https://www.taptap.cn/app/384964) | 2024-04-15 |

# 贡献名单

<!-- readme: contributors -start -->
<table>
	<tbody>
		<tr>
            <td align="center">
                <a href="https://github.com/AlianBlank">
                    <img src="https://avatars.githubusercontent.com/u/1950044?v=4" width="100;" alt="AlianBlank"/>
                    <br />
                    <sub><b>Blank</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/blankalian">
                    <img src="https://avatars.githubusercontent.com/u/147848600?v=4" width="100;" alt="blankalian"/>
                    <br />
                    <sub><b>blankalian</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/bambom">
                    <img src="https://avatars.githubusercontent.com/u/11567449?v=4" width="100;" alt="bambom"/>
                    <br />
                    <sub><b>bambom</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/PlayerYF">
                    <img src="https://avatars.githubusercontent.com/u/56374327?v=4" width="100;" alt="PlayerYF"/>
                    <br />
                    <sub><b>PlayerYF</b></sub>
                </a>
            </td>
		</tr>
	<tbody>
</table>
<!-- readme: contributors -end -->

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=AlianBlank/GameFrameX,AlianBlank/GameFrameX.Unity,AlianBlank/GameFrameX.Server,AlianBlank/GameFrameX.Admin&type=Date)](https://star-history.com/embed?secret=Z2hwX0l1VlJVYlE0RUhIZE9hS2pVZ21ISVozNFNNSUdETDMycmZEWQ==#GameFrameX/GameFrameX&GameFrameX/GameFrameX.Unity&GameFrameX/GameFrameX.Server&GameFrameX/GameFrameX.Admin&Date)

# 免责声明

所有插件均来自互联网.请各位使用时自行付费.如果以上插件涉及侵权.请发email.本人将移除.谢谢

该项目不得用于当地法律不允许的使用范围.如果使用.本人或本组织将不承认和承担任何的法律责任和条款约束.

技术本无罪,错的是滥用技术的人
