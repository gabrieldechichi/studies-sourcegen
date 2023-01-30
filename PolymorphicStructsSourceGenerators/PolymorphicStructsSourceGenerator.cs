using System;
using System.Collections.Generic;
using System.Linq;
using Core.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolymorphicStructs
{
    /// <summary>
    /// Base class implementing an interface
    /// </summary>
    internal class BaseStructDef
    {
        public string Name;
        public string InterfaceName;
        public string Namespace;
        public List<string> UsingDirectives;
        public List<ISymbol> InterfaceMethods;

        public void TryAddUniqueUsing(string usingDirective)
        {
            if (!string.IsNullOrEmpty(usingDirective) && !string.IsNullOrWhiteSpace(usingDirective) &&
                !UsingDirectives.Contains(usingDirective))
            {
                UsingDirectives.Add(usingDirective);
            }
        }

        public void TryAddManyUniqueUsings(IEnumerable<UsingDirectiveSyntax> usingDirectives)
        {
            foreach (var u in usingDirectives)
            {
                TryAddUniqueUsing(u.Name.ToString());
            }
        }
    }

    /// <summary>
    /// A struct implementing an interface
    /// </summary>
    internal class StructImplDef
    {
        public string Name;
        public string Namespace;
        public List<StructField> Fields;
    }

    internal class StructField
    {
        public ITypeSymbol Type;
        public string FieldName;
        public string BaseStructFieldName;
    }

    internal class MergedStructField
    {
        public ITypeSymbol Type;
        public string FieldName;
        public Dictionary<StructImplDef, string> StructToFieldName;
    }

    [Generator]
    internal class PolymorphicStructsSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // SourceGenUtils.AttachDebugger();
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
            var structImplDefs = BuildStructImplDefs(context, syntaxReceiver, baseStructDef);
            if (structImplDefs.Count == 0)
            {
                return;
            }

            var mergedFields = BuildMergedStructFields(structImplDefs);

            WriteBaseStructSource(baseStructDef, mergedFields, context);
        }


        private BaseStructDef BuildBaseStructForInterface(GeneratorExecutionContext context,
            InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            var baseStructDef = new BaseStructDef
            {
                Name = interfaceDeclarationSyntax.Identifier.Text.TrimStart('I'),
                InterfaceName = interfaceDeclarationSyntax.Identifier.Text,
                Namespace = interfaceDeclarationSyntax.GetNamespace(),
                InterfaceMethods = interfaceDeclarationSyntax.GetAllMethods(context).ToList()
            };

            baseStructDef.UsingDirectives = interfaceDeclarationSyntax.SyntaxTree
                .GetCompilationUnitRoot(context.CancellationToken).Usings
                .Select(u => u.Name.ToString()).ToList();

            baseStructDef.TryAddUniqueUsing("System");
            baseStructDef.TryAddUniqueUsing(baseStructDef.Namespace);

            return baseStructDef;
        }

        private List<StructImplDef> BuildStructImplDefs(GeneratorExecutionContext context,
            PolymorphicStructSyntaxReceiver syntaxReceiver, BaseStructDef baseStructDef)
        {
            var retVal = new List<StructImplDef>();
            var structsImplementingInterface =
                syntaxReceiver.AllStructsImplementingInterfaces.Where(s =>
                    s.ImplementsInterface(baseStructDef.InterfaceName) &&
                    !s.Identifier.Text.Equals(baseStructDef.Name));
            foreach (var s in structsImplementingInterface)
            {
                baseStructDef.TryAddManyUniqueUsings(s.SyntaxTree.GetCompilationUnitRoot(context.CancellationToken)
                    .Usings);
                var structImpl = new StructImplDef
                {
                    Name = s.Identifier.ToString(),
                    Namespace = s.GetNamespace(),
                    Fields = s.GetAllFields(context).Select(f => new StructField
                    {
                        Type = f.Type,
                        FieldName = f.Name
                    }).ToList()
                };

                retVal.Add(structImpl);
            }

            return retVal;
        }

        private List<MergedStructField> BuildMergedStructFields(List<StructImplDef> structs)
        {
            var mergedFields = new List<MergedStructField>();
            var usedIndexesInMergedFields = new List<int>();

            int FindOrCreateMergedField(StructField field)
            {
                var matchingFieldIndex = -1;
                for (var i = 0; i < mergedFields.Count; i++)
                {
                    if (mergedFields[i].Type.Equals(field.Type, SymbolEqualityComparer.Default)
                        && !usedIndexesInMergedFields.Contains(i))
                    {
                        matchingFieldIndex = i;
                        break;
                    }
                }

                if (matchingFieldIndex < 0)
                {
                    var mergedField = new MergedStructField
                    {
                        Type = field.Type,
                        FieldName = $"{field.Type.Name}_{mergedFields.Count}",
                        StructToFieldName = new Dictionary<StructImplDef, string>()
                    };
                    mergedFields.Add(mergedField);
                    matchingFieldIndex = mergedFields.Count - 1;
                }

                return matchingFieldIndex;
            }
            
            foreach (var structImpl in structs)
            {
                usedIndexesInMergedFields.Clear();
                foreach (var field in structImpl.Fields)
                {
                    var mergedFieldIndex = FindOrCreateMergedField(field);
                    var mergedField = mergedFields[mergedFieldIndex];
                    mergedField.StructToFieldName.Add(structImpl, field.FieldName);
                    field.BaseStructFieldName = mergedField.FieldName;
                    usedIndexesInMergedFields.Add(mergedFieldIndex);
                }
            }

            return mergedFields;
        }

        private void WriteBaseStructSource(BaseStructDef baseStructDef, List<MergedStructField> mergedFields, GeneratorExecutionContext context)
        {
            var sourceWriter = new SourceWriter();
            sourceWriter.WriteUsings(baseStructDef.UsingDirectives);
            using (sourceWriter.WithNamespace(baseStructDef.Namespace))
            {
                sourceWriter.WriteLine($"public struct {baseStructDef.Name}");
                using (sourceWriter.WithScope())
                {
                    foreach (var f in mergedFields)
                    {
                        sourceWriter.WriteLine($"public {f.Type.Name} {f.FieldName};");
                    }
                }
            }

            var genText = sourceWriter.StringBuilder.ToString();
            context.AddSource($"{baseStructDef.Name}.gen.cs", genText);
        }
    }
}