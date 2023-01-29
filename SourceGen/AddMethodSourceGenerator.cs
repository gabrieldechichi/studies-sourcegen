using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace SourceGenerator
{
    [Generator]
    public class AddMethodSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new GenerateMethodSyntaxReceiver());

// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //
            var syntaxReceiver = (GenerateMethodSyntaxReceiver)context.SyntaxReceiver;

            foreach (var cds in syntaxReceiver.ClassesToGenerateMethod)
            {
                var cdsNamespace = cds.FindNamespace();
                var sourceText = $@"


namespace {cdsNamespace.Name.ToString()}
{{
    public partial class {cds.Identifier}
    {{
        public void MyGeneratedMethod()
        {{
            Console.WriteLine($""Hello from generated code {{nameof({cds.Identifier})}}"");
        }}
    }}
}}
";
                context.AddSource($"{cds.Identifier}.Generated.cs", sourceText);
            }
        }

        class GenerateMethodSyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> ClassesToGenerateMethod = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax cds &&
                    cds.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == "GenerateMethodAtribute")))
                {
                    ClassesToGenerateMethod.Add(cds);
                }
            }
        }
    }
}