// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0002：StateComponent 子类必须定义在 GameFrameX.Apps 程序集中。
/// </summary>
/// <remarks>
/// <para>规则：所有继承自 StateComponent&lt;TState&gt; 的类必须位于 GameFrameX.Apps 程序集。</para>
/// <para>原因：StateComponent 持有 CacheState 的引用，与状态数据紧密绑定。
/// 若将 StateComponent 放在 Hotfix 中，热重载时组件实例会被重建，
/// 但关联的 CacheState 仍在 Apps 层，导致引用断裂和状态丢失。</para>
/// <para>目的：确保 Component+State 组合始终在状态层中共存，维护 ECS 架构中状态的完整性。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StateComponentAssemblyAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0002",
        "StateComponent must be defined in GameFrameX.Apps",
        "Type '{0}' inherits StateComponent<TState> and must be defined in assembly '{1}', not '{2}'",
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
        if (!ArchitectureSymbolFacts.InheritsFrom(type, symbols.StateComponent) || assemblyName == ArchitectureAnalyzerConstants.AppsAssembly)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), ArchitectureAnalyzerConstants.AppsAssembly, assemblyName);
    }
}
