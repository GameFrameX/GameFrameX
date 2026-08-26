// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//  ==========================================================================================

#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GameFrameX.Architecture.Analyzers;

/// <summary>
/// 架构分析器共用的符号判断工具方法。
/// </summary>
/// <remarks>
/// 提供 类型继承、接口实现、属性标注、程序集过滤 等判断逻辑，
/// 供所有分析器在 AnalyzeNamedType 中调用。
/// </remarks>
internal static class ArchitectureSymbolFacts
{
    /// <summary>
    /// 判断类型是否为需要分析的候选类。
    /// 排除非 Class 类型（struct/enum/interface 等）和编译器隐式生成的类型（如匿名类、闭包类）。
    /// </summary>
    public static bool IsCandidateClass(INamedTypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class && !type.IsImplicitlyDeclared;
    }

    /// <summary>
    /// 判断程序集是否应被所有分析器跳过。
    /// </summary>
    /// <remarks>
    /// 跳过规则：
    ///   - GameFrameX.Proto  ：Proto 消息定义，不含业务逻辑。
    ///   - GameFrameX.Config ：配置定义，不含业务逻辑。
    ///   - *.Tests / *.Tests.*：单元测试程序集，测试中可能需要违反架构规则进行 mock。
    /// </remarks>
    public static bool ShouldIgnoreAssembly(string assemblyName)
    {
        return assemblyName == "GameFrameX.Proto"
               || assemblyName == "GameFrameX.Config"
               || assemblyName.EndsWith(".Tests", StringComparison.Ordinal)
               || assemblyName.IndexOf(".Tests.", StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// 判断类型是否继承自指定基类（沿继承链向上搜索，不含 object）。
    /// </summary>
    public static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol? expectedBaseType)
    {
        if (expectedBaseType == null)
        {
            return false;
        }

        for (var current = type.BaseType; current != null; current = current.BaseType)
        {
            if (SymbolEquals(current, expectedBaseType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断类型是否实现了指定接口（含所有已声明的接口，包括继承的）。
    /// </summary>
    public static bool Implements(INamedTypeSymbol type, INamedTypeSymbol? expectedInterface)
    {
        if (expectedInterface == null)
        {
            return false;
        }

        foreach (var candidate in type.AllInterfaces)
        {
            if (SymbolEquals(candidate, expectedInterface))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断类型是否标注了指定特性（Attribute）。
    /// </summary>
    public static bool HasAttribute(INamedTypeSymbol type, INamedTypeSymbol? expectedAttribute)
    {
        if (expectedAttribute == null)
        {
            return false;
        }

        foreach (var attribute in type.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass != null && SymbolEquals(attributeClass, expectedAttribute))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断两个类型符号是否代表同一个类型（比较 OriginalDefinition 以忽略泛型参数差异）。
    /// </summary>
    public static bool SymbolEquals(INamedTypeSymbol? left, INamedTypeSymbol? right)
    {
        if (left == null || right == null)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(left.OriginalDefinition, right.OriginalDefinition);
    }

    /// <summary>
    /// 判断类型是否为源生成器自动生成的 ComponentAgentWrapper。
    /// </summary>
    /// <remarks>
    /// 源生成器会为每个 ComponentAgent 生成一个 Wrapper 类，名称以 "ComponentAgentWrapper" 结尾。
    /// 这些生成类不应被分析器检查，因为它们是自动产物而非手写代码。
    /// </remarks>
    public static bool IsGeneratedComponentAgentWrapper(INamedTypeSymbol type)
    {
        return type.Name.EndsWith("ComponentAgentWrapper", StringComparison.Ordinal);
    }

    /// <summary>
    /// 向编译器报告一个诊断错误，定位到类型声明位置。
    /// </summary>
    public static void Report(SymbolAnalysisContext context, DiagnosticDescriptor descriptor, INamedTypeSymbol type, params object[] args)
    {
        context.ReportDiagnostic(Diagnostic.Create(descriptor, type.Locations.FirstOrDefault(), args));
    }
}
