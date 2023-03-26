using System.Collections.Generic;
using Core.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolymorphicStructs
{
    internal class PolymorphicStructSyntaxReceiver : ISyntaxReceiver
    {
        public List<InterfaceDeclarationSyntax> PolymorphicStructInterfaces = new List<InterfaceDeclarationSyntax>();
        public List<StructDeclarationSyntax> AllStructsImplementingInterfaces = new List<StructDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax interfaceNode &&
                interfaceNode.HasAttribute(PolymorphicStructsConstants.PolymorphicStructInterfaceName))
            {
                PolymorphicStructInterfaces.Add(interfaceNode);
            }
            //at this time we can't know all the PolymorphicStructInterfaces, so we get any struct that implements an interface
            //because structs don't support inheritance, any struct with BaseList.Types > 0 is implementing an interface
            else if (syntaxNode is StructDeclarationSyntax structNode && structNode.BaseList != null &&
                     structNode.BaseList.Types.Any())
            {
                AllStructsImplementingInterfaces.Add(structNode);
            }
        }
    }
}