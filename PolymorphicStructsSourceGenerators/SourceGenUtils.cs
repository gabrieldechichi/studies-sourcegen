using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;

namespace Core.SourceGen
{
    public static class SourceGenUtils
    {
        public static void AttachDebugger()
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }

        public static bool HasAttribute(this BaseTypeDeclarationSyntax typeSyntax, string attributeName)
        {
            if (typeSyntax.AttributeLists != null)
            {
                foreach (AttributeListSyntax attributeList in typeSyntax.AttributeLists)
                {
                    foreach (AttributeSyntax attribute in attributeList.Attributes)
                    {
                        if (attribute.Name.ToString() == attributeName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool ImplementsInterface(this BaseTypeDeclarationSyntax typeSyntax, string interfaceName)
        {
            if (typeSyntax.BaseList != null)
            {
                foreach (BaseTypeSyntax type in typeSyntax.BaseList.Types)
                {
                    if (type.ToString() == interfaceName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static IEnumerable<MethodDeclarationSyntax> GetAllMethodsOf(TypeDeclarationSyntax t)
        {
            IEnumerable<MethodDeclarationSyntax> methods = t.Members
                .Where(m => m.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>();

            return methods;
        }

        public static IEnumerable<PropertyDeclarationSyntax> GetAllPropertiesOf(TypeDeclarationSyntax t)
        {
            IEnumerable<PropertyDeclarationSyntax> properties = t.Members
                .Where(m => m.IsKind(SyntaxKind.PropertyDeclaration)).OfType<PropertyDeclarationSyntax>();

            return properties;
        }

        public static IEnumerable<FieldDeclarationSyntax> GetAllFieldsOf(TypeDeclarationSyntax t)
        {
            IEnumerable<FieldDeclarationSyntax> fields = t.Members
                .Where(m => m.IsKind(SyntaxKind.FieldDeclaration)).OfType<FieldDeclarationSyntax>();

            return fields;
        }

        public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
        {
            string nameSpace = string.Empty;
            SyntaxNode potentialNamespaceParent = syntax.Parent;

            while (potentialNamespaceParent != null && !(potentialNamespaceParent is NamespaceDeclarationSyntax))
            {
                potentialNamespaceParent = potentialNamespaceParent.Parent;
            }

            if (potentialNamespaceParent != null &&
                potentialNamespaceParent is NamespaceDeclarationSyntax namespaceParent)
            {
                nameSpace = namespaceParent.Name.ToString();
            }

            return nameSpace;
        }

        public static IEnumerable<IMethodSymbol> GetAllMethods(this InterfaceDeclarationSyntax polymorphicInterface,
            GeneratorExecutionContext context)
        {
            var semanticModel = context.Compilation.GetSemanticModel(polymorphicInterface.SyntaxTree);
            var interfaceSymbol = semanticModel.GetDeclaredSymbol(polymorphicInterface, context.CancellationToken);

            return interfaceSymbol?.GetMembers().Concat(interfaceSymbol
                    .AllInterfaces
                    .SelectMany(it => it.GetMembers()))
                .OfType<IMethodSymbol>();
        }

        public static IEnumerable<IFieldSymbol> GetAllFields(this BaseTypeDeclarationSyntax type, GeneratorExecutionContext context)
        {
            var semanticModel = context.Compilation.GetSemanticModel(type.SyntaxTree);
            var typeSymbol = semanticModel.GetDeclaredSymbol(type, context.CancellationToken);
            return typeSymbol?.GetMembers().Where(m => m.Kind == SymbolKind.Field).Cast<IFieldSymbol>();
        }

        private static bool IsNotAPropertyMethod(ISymbol it)
        {
            return !(it is IMethodSymbol methodSymbol) || methodSymbol.MethodKind != MethodKind.PropertyGet &&
                methodSymbol.MethodKind != MethodKind.PropertySet;
        }

        public static string BuildInvokeString(this IMethodSymbol method, string objName)
        {
            return $"{objName}.{method.Name}({method.BuildParameterListForInvocation()})";
        }
        
        public static string BuildParameterListForInvocation(this IMethodSymbol method)
        {
            return string.Join(", ",
                method.Parameters.Select(p => $"{p.RefKind.RefKindToSourceString()}{p.Name}"));
        }
        public static string BuildParameterListForDeclaration(this IMethodSymbol method)
        {
            return string.Join(", ",
                method.Parameters.Select(p => $"{p.RefKind.RefKindToSourceString()}{p.Type} {p.Name}"));
        }
        
        public static string RefKindToSourceString(this RefKind argRefKind)
        {
            switch(argRefKind)
            {
                case RefKind.None:
                    return "";
                case RefKind.Ref:
                    return "ref ";
                case RefKind.Out:
                    return "out ";
                case RefKind.In:
                    return "in ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(argRefKind), argRefKind, null);
            }
        }
    }
}