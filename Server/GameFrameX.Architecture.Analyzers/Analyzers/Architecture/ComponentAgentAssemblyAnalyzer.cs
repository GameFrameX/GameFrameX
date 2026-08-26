// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0005：IComponentAgent 实现类必须定义在 GameFrameX.Hotfix 程序集中。
/// </summary>
/// <remarks>
/// <para>规则：所有非抽象的 IComponentAgent 实现类（排除源生成器生成的 ComponentAgentWrapper）必须位于 GameFrameX.Hotfix 程序集。</para>
/// <para>原因：ComponentAgent 是 ECS 架构中 System 的对应物，承载组件的业务逻辑。
/// 它必须在可热更新的 Hotfix 层中，以便在不重启服务器的情况下修复业务逻辑。</para>
/// <para>目的：确保组件逻辑可热更新，实现不停机修复。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ComponentAgentAssemblyAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0005",
        "Component agent must be defined in GameFrameX.Hotfix",
        "Type '{0}' implements IComponentAgent and must be defined in assembly '{1}', not '{2}'",
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
        if (type.IsAbstract
            || !ArchitectureSymbolFacts.Implements(type, symbols.ComponentAgent)
            || ArchitectureSymbolFacts.IsGeneratedComponentAgentWrapper(type)
            || assemblyName == ArchitectureAnalyzerConstants.HotfixAssembly)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), ArchitectureAnalyzerConstants.HotfixAssembly, assemblyName);
    }
}
