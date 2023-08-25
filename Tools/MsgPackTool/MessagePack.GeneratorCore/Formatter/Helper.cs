using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MessagePackCompiler
{
    public static class Helper
    {
        private static bool Implements(INamedTypeSymbol symbol, ITypeSymbol type)
        {
            return symbol.AllInterfaces.Any(i => type.Equals(i));
        }

        private static bool InheritsFrom(INamedTypeSymbol symbol, ITypeSymbol type)
        {
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (type.Equals(baseType))
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }


        public static IEnumerable<INamedTypeSymbol> GetTypesByMetadataName(this Compilation compilation, string typeMetadataName)
        {
            return compilation.References
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .Select(assemblySymbol => assemblySymbol.GetTypeByMetadataName(typeMetadataName))
                .Where(t => t != null);
        }


        public static bool TryGetParentSyntax<T>(this SyntaxNode syntaxNode, out T result) where T : SyntaxNode
        {
            // set defaults
            result = null;

            if (syntaxNode == null)
            {
                return false;
            }

            try
            {
                syntaxNode = syntaxNode.Parent;

                if (syntaxNode == null)
                {
                    return false;
                }

                if (syntaxNode.GetType() == typeof(T))
                {
                    result = syntaxNode as T;
                    return true;
                }

                return TryGetParentSyntax<T>(syntaxNode, out result);
            }
            catch
            {
                return false;
            }
        }

        //public static string GetFullName(this ClassDeclarationSyntax clsSyntas)
        //{
        //    NamespaceDeclarationSyntax namespaceDeclarationSyntax = null;
        //    if (!TryGetParentSyntax(clsSyntas, out namespaceDeclarationSyntax))
        //    {
        //        return null; // or whatever you want to do in this scenario
        //    }
        //    var namespaceName = namespaceDeclarationSyntax.Name.ToString();
        //    var fullClassName = namespaceName + "." + clsSyntas.Identifier.ToString();
        //    return fullClassName;
        //}

        public static string GetFullName(this BaseTypeDeclarationSyntax syntax)
        {
            if (TryGetParentSyntax(syntax, out NamespaceDeclarationSyntax namespaceDeclarationSyntax))
            {
                var namespaceName = namespaceDeclarationSyntax.Name.ToString();
                var fullClassName = namespaceName + "." + syntax.Identifier.ToString();
                return fullClassName;
            }
            return syntax.Identifier.ToString();
        }

        public static string GetNameSpace(this BaseTypeDeclarationSyntax syntax)
        {
            if (TryGetParentSyntax(syntax, out NamespaceDeclarationSyntax namespaceDeclarationSyntax))
                return namespaceDeclarationSyntax.Name.ToString();
            return null;
        }

    }
}
