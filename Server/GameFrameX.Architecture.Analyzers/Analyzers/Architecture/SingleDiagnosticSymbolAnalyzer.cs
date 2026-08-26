// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//  ==========================================================================================

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// 单诊断符号分析器的抽象基类。
/// </summary>
/// <remarks>
/// 封装了 Roslyn DiagnosticAnalyzer 的通用初始化逻辑：
///   1. 跳过生成的代码（GeneratedCodeAnalysisFlags.None）。
///   2. 启用并发执行以提高分析性能。
///   3. 在编译开始时创建 <see cref="ArchitectureSymbols"/> 并缓存。
///   4. 注册 NamedType 级别的符号分析，自动过滤非候选类型和应忽略的程序集。
///
/// 子类只需实现 <see cref="Descriptor"/>（描述哪个诊断）和
/// <see cref="AnalyzeNamedType"/>（具体检查逻辑）即可。
/// </remarks>
public abstract class SingleDiagnosticSymbolAnalyzer : DiagnosticAnalyzer
{
    protected abstract DiagnosticDescriptor Descriptor { get; }

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get { return ImmutableArray.Create(Descriptor); }
    }

    public sealed override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterCompilation);
    }

    protected abstract void AnalyzeNamedType(SymbolAnalysisContext context, ArchitectureSymbols symbols, INamedTypeSymbol type);

    private void RegisterCompilation(CompilationStartAnalysisContext context)
    {
        var symbols = ArchitectureSymbols.Create(context.Compilation);
        context.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext, symbols), SymbolKind.NamedType);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context, ArchitectureSymbols symbols)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!ArchitectureSymbolFacts.IsCandidateClass(type))
        {
            return;
        }

        if (ArchitectureSymbolFacts.ShouldIgnoreAssembly(type.ContainingAssembly.Identity.Name))
        {
            return;
        }

        AnalyzeNamedType(context, symbols, type);
    }
}
