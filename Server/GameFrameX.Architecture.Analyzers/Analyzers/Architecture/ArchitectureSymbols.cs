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
//  ==========================================================================================

#nullable enable

using Microsoft.CodeAnalysis;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// 从编译上下文中一次性解析所有架构分析器需要引用的"知名类型"符号。
/// </summary>
/// <remarks>
/// 在编译开始时通过 <see cref="Create"/> 创建一次，随后在所有分析器之间共享。
/// 属性值可能为 null（当被引用的程序集未被项目引用时），各分析器需做 null 安全处理。
/// </remarks>
public sealed class ArchitectureSymbols
{
    private ArchitectureSymbols(
        INamedTypeSymbol? baseCacheState,
        INamedTypeSymbol? cacheState,
        INamedTypeSymbol? stateComponent,
        INamedTypeSymbol? baseHttpHandler,
        INamedTypeSymbol? hotfixBridge,
        INamedTypeSymbol? componentAgent,
        INamedTypeSymbol? messageMappingAttribute,
        INamedTypeSymbol? messageRpcMappingAttribute,
        INamedTypeSymbol? eventListener,
        INamedTypeSymbol? timerHandler)
    {
        BaseCacheState = baseCacheState;
        CacheState = cacheState;
        StateComponent = stateComponent;
        BaseHttpHandler = baseHttpHandler;
        HotfixBridge = hotfixBridge;
        ComponentAgent = componentAgent;
        MessageMappingAttribute = messageMappingAttribute;
        MessageRpcMappingAttribute = messageRpcMappingAttribute;
        EventListener = eventListener;
        TimerHandler = timerHandler;
    }

    /// <summary>GameFrameX.DataBase.BaseCacheState — 数据库缓存状态的抽象基类。</summary>
    public INamedTypeSymbol? BaseCacheState { get; }

    /// <summary>GameFrameX.DataBase.Mongo.CacheState — MongoDB 缓存状态的默认实现（分析时跳过此类型自身）。</summary>
    public INamedTypeSymbol? CacheState { get; }

    /// <summary>GameFrameX.Core.Components.StateComponent`1 — 持有 CacheState 的状态组件基类。</summary>
    public INamedTypeSymbol? StateComponent { get; }

    /// <summary>GameFrameX.NetWork.HTTP.BaseHttpHandler — HTTP 请求处理器的基类。</summary>
    public INamedTypeSymbol? BaseHttpHandler { get; }

    /// <summary>GameFrameX.Core.Hotfix.IHotfixBridge — 热更新桥接接口，用于状态层调用逻辑层。</summary>
    public INamedTypeSymbol? HotfixBridge { get; }

    /// <summary>GameFrameX.Core.Abstractions.Agent.IComponentAgent — 组件代理接口（ECS 中的 System 角色）。</summary>
    public INamedTypeSymbol? ComponentAgent { get; }

    /// <summary>GameFrameX.NetWork.Abstractions.MessageMappingAttribute — 标记消息处理器的特性。</summary>
    public INamedTypeSymbol? MessageMappingAttribute { get; }

    /// <summary>GameFrameX.NetWork.Abstractions.MessageRpcMappingAttribute — 标记 RPC 消息处理器的特性。</summary>
    public INamedTypeSymbol? MessageRpcMappingAttribute { get; }

    /// <summary>GameFrameX.Core.Abstractions.Events.IEventListener — 事件监听器接口。</summary>
    public INamedTypeSymbol? EventListener { get; }

    /// <summary>GameFrameX.Core.Timer.Handler.ITimerHandler — 定时器回调处理器接口。</summary>
    public INamedTypeSymbol? TimerHandler { get; }

    /// <summary>
    /// 从编译上下文中解析所有知名类型符号。每个编译只调用一次。
    /// </summary>
    public static ArchitectureSymbols Create(Compilation compilation)
    {
        return new ArchitectureSymbols(
            compilation.GetTypeByMetadataName("GameFrameX.DataBase.BaseCacheState"),
            compilation.GetTypeByMetadataName("GameFrameX.DataBase.Mongo.CacheState"),
            compilation.GetTypeByMetadataName("GameFrameX.Core.Components.StateComponent`1"),
            compilation.GetTypeByMetadataName("GameFrameX.NetWork.HTTP.BaseHttpHandler"),
            compilation.GetTypeByMetadataName("GameFrameX.Core.Hotfix.IHotfixBridge"),
            compilation.GetTypeByMetadataName("GameFrameX.Core.Abstractions.Agent.IComponentAgent"),
            compilation.GetTypeByMetadataName("GameFrameX.NetWork.Abstractions.MessageMappingAttribute"),
            compilation.GetTypeByMetadataName("GameFrameX.NetWork.Abstractions.MessageRpcMappingAttribute"),
            compilation.GetTypeByMetadataName("GameFrameX.Core.Abstractions.Events.IEventListener"),
            compilation.GetTypeByMetadataName("GameFrameX.Core.Timer.Handler.ITimerHandler"));
    }
}
