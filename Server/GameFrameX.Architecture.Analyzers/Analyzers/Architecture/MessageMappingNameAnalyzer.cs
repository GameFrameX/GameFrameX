// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0012：标记了 MessageMappingAttribute 的类型名称必须以 "Handler" 结尾。
/// </summary>
/// <remarks>
/// <para>规则：所有标注了 MessageMappingAttribute 的类型的名称必须以 "Handler" 结尾。</para>
/// <para>原因：统一的命名约定让开发者一眼识别出消息处理器的职责，
/// 也便于源生成器和文档工具按约定发现处理器类型。</para>
/// <para>目的：强制执行命名约定，提高代码可读性和可维护性。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MessageMappingNameAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0012",
        "MessageMapping handler name must end with Handler",
        "MessageMapping handler '{0}' must end with 'Handler'",
        ArchitectureAnalyzerConstants.Category,
        DiagnosticSeverity.Error,
        true);

    protected override DiagnosticDescriptor Descriptor
    {
        get { return SDescriptor; }
    }

    protected override void AnalyzeNamedType(SymbolAnalysisContext context, ArchitectureSymbols symbols, INamedTypeSymbol type)
    {
        if (!ArchitectureSymbolFacts.HasAttribute(type, symbols.MessageMappingAttribute) || type.Name.EndsWith("Handler", StringComparison.Ordinal))
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
    }
}
