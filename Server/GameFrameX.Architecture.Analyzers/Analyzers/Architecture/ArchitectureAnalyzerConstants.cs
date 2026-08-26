// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 Apache License 2.0 单协议分发，
//   This project is licensed solely under the Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root directory of the source code for the full license text.
//   禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//   It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//   侵犯他人合法权益等法律法规所禁止的行为！
//   or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//   因基于本项目二次开发所产生的一切法律纠纷与责任，
//   Any legal disputes and liabilities arising from secondary development based on this project
//   本项目组织与贡献者概不承担。
//   shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//   GitHub 仓库：https://github.com/GameFrameX
//   GitHub Repository: https://github.com/GameFrameX
//   Gitee  仓库：https://gitee.com/GameFrameX
//   Gitee Repository:  https://gitee.com/GameFrameX
//   CNB  仓库：https://cnb.cool/GameFrameX
//   CNB Repository:  https://cnb.cool/GameFrameX
//   官方文档：https://gameframex.doc.alianblank.com/
//   Official Documentation: https://gameframex.doc.alianblank.com/
//  ==========================================================================================

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// 架构分析器使用的全局常量。
/// </summary>
/// <remarks>
/// GameFrameX 采用三层程序集架构：
///   - GameFrameX.Apps   ：状态层，存放 CacheState / StateComponent 等持久化数据结构。
///   - GameFrameX.Hotfix ：逻辑层，存放所有业务处理代码（Handler、Agent、Listener 等）。
///   - 其他程序集        ：基础设施层（网络、数据库、Proto 定义等），不允许包含业务逻辑。
///
/// 这些常量被所有分析器共享，用于判断类型的归属程序集是否正确。
/// </remarks>
internal static class ArchitectureAnalyzerConstants
{
    /// <summary>诊断类别标识，所有架构分析器共用此类别。</summary>
    public const string Category = "GameFrameX.Architecture";

    /// <summary>状态层程序集名称。CacheState 和 StateComponent 必须定义在此程序集中。</summary>
    public const string AppsAssembly = "GameFrameX.Apps";

    /// <summary>逻辑层程序集名称。Handler、Agent、Listener 等业务类型必须定义在此程序集中。</summary>
    public const string HotfixAssembly = "GameFrameX.Hotfix";
}
