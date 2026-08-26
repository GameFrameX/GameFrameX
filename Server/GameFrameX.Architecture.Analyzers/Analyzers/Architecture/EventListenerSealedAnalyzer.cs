// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0011：IEventListener 实现类必须标记为 sealed。
/// </summary>
/// <remarks>
/// <para>规则：所有非抽象的 IEventListener 实现类必须使用 sealed 修饰符。</para>
/// <para>原因：事件监听器通过源生成器自动注册到事件总线。若允许继承，子类也会被识别为监听器，
/// 导致同一事件被多个类型重复处理，产生难以排查的 bug。</para>
/// <para>目的：防止事件监听器被继承导致的重复注册问题，同时允许编译器进行虚调用优化。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventListenerSealedAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0011",
        "Event listener must be sealed",
        "Event listener '{0}' must be sealed",
        ArchitectureAnalyzerConstants.Category,
        DiagnosticSeverity.Error,
        true);

    protected override DiagnosticDescriptor Descriptor
    {
        get { return SDescriptor; }
    }

    protected override void AnalyzeNamedType(SymbolAnalysisContext context, ArchitectureSymbols symbols, INamedTypeSymbol type)
    {
        if (type.IsAbstract || !ArchitectureSymbolFacts.Implements(type, symbols.EventListener) || type.IsSealed)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
    }
}
