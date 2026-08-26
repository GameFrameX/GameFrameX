// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0004：IHotfixBridge 实现类必须定义在 GameFrameX.Hotfix 程序集中。
/// </summary>
/// <remarks>
/// <para>规则：所有实现了 IHotfixBridge 接口的类必须位于 GameFrameX.Hotfix 程序集。</para>
/// <para>原因：IHotfixBridge 是热更新桥接机制的核心接口，用于让状态层（Apps）调用逻辑层（Hotfix）的功能。
/// 桥接实现类必须在 Hotfix 中，否则热更新时无法替换桥接逻辑。</para>
/// <para>目的：确保桥接逻辑可被热更新替换，实现状态层与逻辑层的解耦。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HotfixBridgeAssemblyAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0004",
        "IHotfixBridge must be defined in GameFrameX.Hotfix",
        "Type '{0}' implements IHotfixBridge and must be defined in assembly '{1}', not '{2}'",
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
        if (!ArchitectureSymbolFacts.Implements(type, symbols.HotfixBridge) || assemblyName == ArchitectureAnalyzerConstants.HotfixAssembly)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), ArchitectureAnalyzerConstants.HotfixAssembly, assemblyName);
    }
}
