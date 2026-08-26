// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0010：标记了 MessageMappingAttribute 的类型必须标记为 sealed。
/// </summary>
/// <remarks>
/// <para>规则：所有标注了 MessageMappingAttribute 的类型必须使用 sealed 修饰符。</para>
/// <para>原因：消息处理器通过源生成器自动注册到消息路由表。若允许继承，
/// 子类也会被识别为消息处理器，可能导致同一条消息被多个处理器重复处理，
/// 或者消息路由目标不明确。</para>
/// <para>目的：防止消息处理器被继承导致的路由歧义，同时允许编译器进行虚调用优化。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MessageMappingSealedAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0010",
        "MessageMapping handler must be sealed",
        "MessageMapping handler '{0}' must be sealed",
        ArchitectureAnalyzerConstants.Category,
        DiagnosticSeverity.Error,
        true);

    protected override DiagnosticDescriptor Descriptor
    {
        get { return SDescriptor; }
    }

    protected override void AnalyzeNamedType(SymbolAnalysisContext context, ArchitectureSymbols symbols, INamedTypeSymbol type)
    {
        if (!ArchitectureSymbolFacts.HasAttribute(type, symbols.MessageMappingAttribute) || type.IsSealed)
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
    }
}
