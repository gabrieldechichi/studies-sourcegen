using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static EnumSourceGen.EnumGeneratorConstants;

namespace EnumSourceGen
{
    internal struct EnumToGenerate
    {
        public string Name;
        public List<string> Values;
    }

    internal static class EnumGeneratorConstants
    {
        public const string EnumExtensionsAttribute = "EnumExtensions";
    }

    [Generator]
    internal class EnumGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            AttachDebugger();
            var enumDeclarations =
                context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: FilterTargetEnums_BroadPass,
                    transform: FilterTargetEnums
                //Note: This is not LINQ, it's from IncrementalValueProviderExtensions
                ).Where(m => m != null);

            var compilationAndEnums = context.CompilationProvider.Combine(enumDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndEnums, (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private bool FilterTargetEnums_BroadPass(SyntaxNode node, CancellationToken cancellationToken)
        {
            //faster check. Just gathers all enums
            return node is EnumDeclarationSyntax enumSyntax && enumSyntax.AttributeLists.Count > 0;
        }

        //This works both as a transform and a filter, we return null if the enum does not contains [EnumExtensions] attribute
        private EnumDeclarationSyntax FilterTargetEnums(GeneratorSyntaxContext ctx,
            CancellationToken cancellationToken)
        {
            var enumDeclarationSyntax = (EnumDeclarationSyntax)ctx.Node;

            foreach (var attributeList in enumDeclarationSyntax.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (attribute.Name.ToString() == EnumExtensionsAttribute)
                    {
                        return enumDeclarationSyntax;
                    }
                }
            }
            return null;
        }

        private static void Execute(Compilation compilationUnit, ImmutableArray<EnumDeclarationSyntax> enums, SourceProductionContext spc)
        {
            if (enums.IsDefaultOrEmpty)
            {
                return;
            }

            var enumsToGenerate = BuildEnumsToGenerate(compilationUnit, enums, spc.CancellationToken);
            var src = GenerateExtensionsClass(enumsToGenerate);
            spc.AddSource("EnumExtensions.g.cs", SourceText.From(src, Encoding.UTF8));
        }

        private static List<EnumToGenerate> BuildEnumsToGenerate(Compilation compilation,
            IEnumerable<EnumDeclarationSyntax> enums, CancellationToken ctx)
        {
            var enumsToGenerate = new List<EnumToGenerate>();

            foreach (var enumDecl in enums)
            {
                var enumToGenerate = new EnumToGenerate();
                enumToGenerate.Name = enumDecl.Identifier.ValueText;
                enumToGenerate.Values = new List<string>(enumDecl.Members.Count);

                foreach (var m in enumDecl.Members)
                {
                    enumToGenerate.Values.Add(m.Identifier.ValueText);
                }

                enumsToGenerate.Add(enumToGenerate);
            }

            return enumsToGenerate;
        }

        private static string GenerateExtensionsClass(List<EnumToGenerate> enumToGenerates)
        {
            var strBuilder = new StringBuilder();

            strBuilder.Append(@"
namespace Game
{
    public static class EnumExtensions
    {");
            foreach (var enumToGenerate in enumToGenerates)
            {
                strBuilder.Append($@"
        public static string ToStringFast(this {enumToGenerate.Name} e)
        {{
            return e switch
            {{");
                foreach (var v in enumToGenerate.Values)
                {
                    strBuilder.Append($@"
                {enumToGenerate.Name}.{v} => nameof({enumToGenerate.Name}.{v}),");
                }

                strBuilder.Append(@"
                _ => throw new Exception()
            };");
            }
            strBuilder.Append(@"
        }");

            strBuilder.Append(@"
    }
}");
            return strBuilder.ToString();
        }

        public static void AttachDebugger()
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }
    }
}