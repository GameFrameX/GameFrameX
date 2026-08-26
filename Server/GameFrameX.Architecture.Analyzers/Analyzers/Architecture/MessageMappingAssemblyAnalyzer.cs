// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0006：标记了 MessageMappingAttribute 的类型必须定义在 GameFrameX.Hotfix 程序集中。
/// </summary>
/// <remarks>
/// <para>规则：所有标注了 MessageMappingAttribute 的类型必须位于 GameFrameX.Hotfix 程序集。</para>
/// <para>原因：消息处理器（MessageMapping）是网络消息到达后的核心业务逻辑入口，
/// 必须在可热更新的 Hotfix 层中，以便在线修复消息处理 bug。</para>
/// <para>目的：确保消息处理逻辑可通过热更新机制快速修复。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MessageMappingAssemblyAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0006",
        "MessageMapping handler must be defined in GameFrameX.Hotfix",
        "Type '{0}' is marked with MessageMappingAttribute and must be defined in assembly '{1}', not '{2}'",
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
        if (!ArchitectureSymbolFacts.HasAttribute(type, symbols.MessageMappingAttribute) || assemblyName == ArchitectureAnalyzerConstants.HotfixAssembly)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), ArchitectureAnalyzerConstants.HotfixAssembly, assemblyName);
    }
}
