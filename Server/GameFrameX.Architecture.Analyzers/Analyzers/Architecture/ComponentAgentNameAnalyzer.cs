// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  ==========================================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// GFX0014：IComponentAgent 实现类的名称必须以 "ComponentAgent" 结尾。
/// </summary>
/// <remarks>
/// <para>规则：所有非抽象的 IComponentAgent 实现类（排除源生成器生成的 ComponentAgentWrapper）的名称必须以 "ComponentAgent" 结尾。</para>
/// <para>原因：源生成器依赖此命名约定来发现 ComponentAgent 并自动生成对应的 Wrapper 类。
/// 若命名不规范，生成器将无法正确发现和注册组件代理。</para>
/// <para>目的：强制统一命名约定，支持源生成器的自动发现机制。</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ComponentAgentNameAnalyzer : SingleDiagnosticSymbolAnalyzer
{
    private static readonly DiagnosticDescriptor SDescriptor = new DiagnosticDescriptor(
        "GFX0014",
        "Component agent name must end with ComponentAgent",
        "Component agent '{0}' must end with 'ComponentAgent'",
        ArchitectureAnalyzerConstants.Category,
        DiagnosticSeverity.Error,
        true);

    protected override DiagnosticDescriptor Descriptor
    {
        get { return SDescriptor; }
    }

    protected override void AnalyzeNamedType(SymbolAnalysisContext context, ArchitectureSymbols symbols, INamedTypeSymbol type)
    {
        if (type.IsAbstract
            || !ArchitectureSymbolFacts.Implements(type, symbols.ComponentAgent)
            || ArchitectureSymbolFacts.IsGeneratedComponentAgentWrapper(type)
            || type.Name.EndsWith("ComponentAgent", StringComparison.Ordinal))
        {
            return;
        }

        ArchitectureSymbolFacts.Report(context, Descriptor, type, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
    }
}
