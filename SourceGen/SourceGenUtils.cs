using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator
{
    public static class SourceGenUtils
    {
        public static NamespaceDeclarationSyntax FindNamespace(this ClassDeclarationSyntax classDeclarationSyntax)
        {
            var parent = classDeclarationSyntax.Parent;
            while (parent != null)
            {
                if (parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                {
                    return namespaceDeclarationSyntax;
                }
                parent = parent.Parent;
            }

            return null;
        }
    }
}