// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0007：标记了 MessageRpcMappingAttribute 的类型必须定义在 GameFrameX.Hotfix 程序集中。
/// </summary>
/// <remarks>
/// <para>规则：所有标注了 MessageRpcMappingAttribute 的类型必须位于 GameFrameX.Hotfix 程序集。</para>
/// <para>原因：RPC 消息处理器与普通消息处理器同理，属于跨进程调用的业务逻辑入口，
/// 必须在可热更新的 Hotfix 层中。</para>
/// <para>目的：确保 RPC 处理逻辑与普通消息处理一样可通过热更新快速修复。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MessageRpcMappingAssemblyAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0007",
        "MessageRpcMapping handler must be defined in GameFrameX.Hotfix",
        "Type '{0}' is marked with MessageRpcMappingAttribute and must be defined in assembly '{1}', not '{2}'",
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
        if (!ArchitectureSymbolFacts.HasAttribute(type, symbols.MessageRpcMappingAttribute) || assemblyName == ArchitectureAnalyzerConstants.HotfixAssembly)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), ArchitectureAnalyzerConstants.HotfixAssembly, assemblyName);
    }
}
