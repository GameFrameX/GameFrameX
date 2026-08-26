// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0013：IEventListener 实现类的名称必须以 "EventListener" 结尾。
/// </summary>
/// <remarks>
/// <para>规则：所有非抽象的 IEventListener 实现类的名称必须以 "EventListener" 结尾。</para>
/// <para>原因：统一的命名约定让事件监听器在代码中一目了然，也便于源生成器按约定发现和注册监听器。</para>
/// <para>目的：强制执行命名约定，提高代码可读性和可维护性，支持自动化注册。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventListenerNameAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0013",
        "Event listener name must end with EventListener",
        "Event listener '{0}' must end with 'EventListener'",
        ArchitectureAnalyzerConstants.Category,
        DiagnosticSeverity.Error,
        true);

    protected override DiagnosticDescriptor Descriptor
    {
        get { return SDescriptor; }
    }

    protected override void AnalyzeNamedType(SymbolAnalysisContext context, ArchitectureSymbols symbols, INamedTypeSymbol type)
    {
        if (type.IsAbstract || !ArchitectureSymbolFacts.Implements(type, symbols.EventListener) || type.Name.EndsWith("EventListener", StringComparison.Ordinal))
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
    }
}
