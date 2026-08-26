// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0003：BaseHttpHandler 子类必须定义在 GameFrameX.Hotfix 程序集中。
/// </summary>
/// <remarks>
/// <para>规则：所有继承自 BaseHttpHandler 的类必须位于 GameFrameX.Hotfix 程序集。</para>
/// <para>原因：HTTP 请求处理属于对外暴露的业务接口逻辑，需要在可热更新的 Hotfix 层中，
/// 以便在发现接口 bug 时无需停服即可修复。</para>
/// <para>目的：确保 HTTP 接口处理逻辑可热更新，保障服务的在线修复能力。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HttpHandlerAssemblyAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0003",
        "BaseHttpHandler must be defined in GameFrameX.Hotfix",
        "Type '{0}' inherits BaseHttpHandler and must be defined in assembly '{1}', not '{2}'",
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
        if (!ArchitectureSymbolFacts.InheritsFrom(type, symbols.BaseHttpHandler) || assemblyName == ArchitectureAnalyzerConstants.HotfixAssembly)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), ArchitectureAnalyzerConstants.HotfixAssembly, assemblyName);
    }
}
