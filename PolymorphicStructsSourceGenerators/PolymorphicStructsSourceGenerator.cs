using System;
using System.Linq;
using Core.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolymorphicStructs
{
    internal class BaseStructDef
    {
        public string Name;
        public string InterfaceName;
        public string Namespace;
        public string[] UsingDirectives;
    }

    [Generator]
    internal class PolymorphicStructsSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            SourceGenUtils.AttachDebugger();
            context.RegisterForSyntaxNotifications(() => new PolymorphicStructSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var syntaxReceiver = (PolymorphicStructSyntaxReceiver)context.SyntaxReceiver;
                foreach (var i in syntaxReceiver.PolymorphicStructInterfaces)
                {
                    GenerateInterfaceCode(context, syntaxReceiver, i);
                }
            }
            catch (Exception e)
            {
                context.WriteExceptionToDiagnostics(e);
            }
        }

        private void GenerateInterfaceCode(GeneratorExecutionContext context,
            PolymorphicStructSyntaxReceiver syntaxReceiver, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            var baseStructDef = BuildBaseStructForInterface(context, interfaceDeclarationSyntax);
            WriteBaseStructSource(baseStructDef, context);
        }

        private BaseStructDef BuildBaseStructForInterface(GeneratorExecutionContext context,
            InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            var baseStructDef = new BaseStructDef
            {
                Name = interfaceDeclarationSyntax.Identifier.Text.TrimStart('I'),
                InterfaceName = interfaceDeclarationSyntax.Identifier.Text,
                Namespace = interfaceDeclarationSyntax.GetNamespace(),
            };

            var usingDirectives = interfaceDeclarationSyntax.SyntaxTree
                .GetCompilationUnitRoot(context.CancellationToken).Usings
                .Select(u => u.Name.ToString())
                .Append("System");
            if (!string.IsNullOrEmpty(baseStructDef.Namespace))
            {
                usingDirectives = usingDirectives.Append(baseStructDef.Namespace);
            }

            baseStructDef.UsingDirectives = usingDirectives.ToArray();
            return baseStructDef;
        }

        private void WriteBaseStructSource(BaseStructDef baseStructDef, GeneratorExecutionContext context)
        {
            var sourceWriter = new SourceWriter();
            sourceWriter.WriteUsings(baseStructDef.UsingDirectives);
            using (sourceWriter.WithNamespace(baseStructDef.Namespace))
            {
                sourceWriter.WriteLine($"public struct {baseStructDef.Name}");
                using (sourceWriter.WithScope())
                {
                }
            }

            var genText = sourceWriter.StringBuilder.ToString();
            context.AddSource($"{baseStructDef.Name}.gen.cs", genText);
        }
    }
}