// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0001：CacheState 子类必须定义在 GameFrameX.Apps 程序集中。
/// </summary>
/// <remarks>
/// <para>规则：所有继承自 BaseCacheState 的非抽象类（排除 CacheState 基类自身）必须位于 GameFrameX.Apps 程序集。</para>
/// <para>原因：CacheState 代表持久化的缓存状态数据，属于状态层。若误放到 Hotfix 等逻辑层程序集，
/// 热重载时状态数据会丢失，导致玩家数据不一致。</para>
/// <para>目的：通过编译期强制隔离，确保状态数据与可热更新的业务逻辑物理分离，保障热更新安全。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CacheStateAssemblyAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0001",
        "CacheState must be defined in GameFrameX.Apps",
        "Type '{0}' inherits CacheState/BaseCacheState and must be defined in assembly '{1}', not '{2}'",
        ArchitectureAnalyzerConstants.Category,
        DiagnosticSeverity.Error,
        true);

    protected override DiagnosticDescriptor Descriptor
    {
        get { return SDescriptor; }
    }

    protected override void AnalyzeNamedType(SymbolAnalysisContext context, ArchitectureSymbols symbols, INamedTypeSymbol type)
    {
        var assemblyName = type.ContainingAssembly.Identity.Name;
        if (!ArchitectureSymbolFacts.InheritsFrom(type, symbols.BaseCacheState)
            || ArchitectureSymbolFacts.SymbolEquals(type, symbols.CacheState)
            || assemblyName == ArchitectureAnalyzerConstants.AppsAssembly)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), ArchitectureAnalyzerConstants.AppsAssembly, assemblyName);
    }
}
