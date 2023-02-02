using System.Collections.Generic;
using System.Linq;
using Core.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static PolymorphicStructs.PolymorphicStructsConstants;

namespace PolymorphicStructs
{
    internal class MergedStructDef
    {
        public string Name;
        public string InterfaceName;
        public string Namespace;
        public List<string> UsingDirectives;
        public List<IMethodSymbol> InterfaceMethods;

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

        public string ToMergedMethodName;
        public List<StructImplField> Fields;
    }

    internal class StructImplField
    {
        public ITypeSymbol Type;
        public string FieldName;
        public string MergedStructFieldName;
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
            var syntaxReceiver = (PolymorphicStructSyntaxReceiver)context.SyntaxReceiver;
            foreach (var i in syntaxReceiver.PolymorphicStructInterfaces)
            {
                GenerateInterfaceCode(context, syntaxReceiver, i);
            }
        }

        private void GenerateInterfaceCode(GeneratorExecutionContext context,
            PolymorphicStructSyntaxReceiver syntaxReceiver, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            var mergedStruct = BuildMergedStructForInterface(context, interfaceDeclarationSyntax);
            var structImplDefs = BuildStructImplDefs(context, syntaxReceiver, mergedStruct);
            if (structImplDefs.Count == 0)
            {
                return;
            }

            var mergedFields = BuildMergedStructFields(structImplDefs);

            var mergedStructSource = GenerateMergedStructSource(mergedStruct, structImplDefs, mergedFields);
            context.AddSource($"{mergedStruct.Name}.gen.cs", mergedStructSource);

            foreach (var s in structImplDefs)
            {
                var implStructSource = GenerateImplStructSource(mergedStruct, s, mergedFields);
                context.AddSource($"{s.Name}.gen.cs", implStructSource);
            }
        }

        private string GenerateImplStructSource(MergedStructDef mergedStructDef, StructImplDef structImplDef,
            List<MergedStructField> mergedFields)
        {
            var sourceWriter = new SourceWriter();
            using (sourceWriter.WithNamespace(structImplDef.Namespace))
            {
                using (sourceWriter.WithNamedScope($"public partial struct {structImplDef.Name}"))
                {
                    //From merge struct (constructor)
                    using (sourceWriter.WithNamedScope($"public {structImplDef.Name}({mergedStructDef.Name} s)"))
                    {
                        foreach (var f in structImplDef.Fields)
                        {
                            sourceWriter.WriteLine($"{f.FieldName} = s.{f.MergedStructFieldName};");
                        }
                    }

                    //To merged struct struct (by ref)
                    using (sourceWriter.WithNamedScope(
                               $"public void {structImplDef.ToMergedMethodName}(ref {mergedStructDef.Name} s)"))
                    {
                        sourceWriter.WriteLine(
                            $"s.{TypeEnumFieldName} = {mergedStructDef.Name}.{TypeEnumName}.{structImplDef.Name};");
                        foreach (var mergedField in mergedFields)
                        {
                            if (mergedField.StructToFieldName.TryGetValue(structImplDef, out var fieldNameInStruct))
                            {
                                sourceWriter.WriteLine($"s.{mergedField.FieldName} = {fieldNameInStruct};");
                            }
                        }
                    }

                    //To merged struct
                    using (sourceWriter.WithNamedScope(
                               $"public {mergedStructDef.Name} {structImplDef.ToMergedMethodName}()"))
                    {
                        sourceWriter.WriteLines(
                            $"var s = new {mergedStructDef.Name}();",
                            $"{structImplDef.ToMergedMethodName}(ref s);",
                            "return s;");
                    }
                }
            }

            return sourceWriter.StringBuilder.ToString();
        }


        private MergedStructDef BuildMergedStructForInterface(GeneratorExecutionContext context,
            InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            var mergedStruct = new MergedStructDef
            {
                Name = interfaceDeclarationSyntax.Identifier.Text.TrimStart('I'),
                InterfaceName = interfaceDeclarationSyntax.Identifier.Text,
                Namespace = interfaceDeclarationSyntax.GetNamespace(),
                InterfaceMethods = interfaceDeclarationSyntax.GetAllMethods(context).ToList()
            };

            mergedStruct.UsingDirectives = interfaceDeclarationSyntax.SyntaxTree
                .GetCompilationUnitRoot(context.CancellationToken).Usings
                .Select(u => u.Name.ToString()).ToList();

            mergedStruct.TryAddUniqueUsing("System");
            mergedStruct.TryAddUniqueUsing(mergedStruct.Namespace);

            return mergedStruct;
        }

        private List<StructImplDef> BuildStructImplDefs(GeneratorExecutionContext context,
            PolymorphicStructSyntaxReceiver syntaxReceiver, MergedStructDef mergedStructDef)
        {
            var retVal = new List<StructImplDef>();
            var structsImplementingInterface =
                syntaxReceiver.AllStructsImplementingInterfaces.Where(s =>
                    s.ImplementsInterface(mergedStructDef.InterfaceName) &&
                    !s.Identifier.Text.Equals(mergedStructDef.Name));
            foreach (var s in structsImplementingInterface)
            {
                mergedStructDef.TryAddManyUniqueUsings(s.SyntaxTree.GetCompilationUnitRoot(context.CancellationToken)
                    .Usings);
                var structImpl = new StructImplDef
                {
                    Name = s.Identifier.ToString(),
                    Namespace = s.GetNamespace(),
                    ToMergedMethodName = $"To{mergedStructDef.Name}",
                    Fields = s.GetAllFields(context).Select(f => new StructImplField
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

            int FindOrCreateMergedField(StructImplField field)
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
                    field.MergedStructFieldName = mergedField.FieldName;
                    usedIndexesInMergedFields.Add(mergedFieldIndex);
                }
            }

            return mergedFields;
        }

        private string GenerateMergedStructSource(MergedStructDef mergedStructDef, List<StructImplDef> structs,
            List<MergedStructField> mergedFields)
        {
            var sourceWriter = new SourceWriter();
            sourceWriter.WriteUsings(mergedStructDef.UsingDirectives);
            using (sourceWriter.WithNamespace(mergedStructDef.Namespace))
            {
                //merged struct definition
                sourceWriter.WriteLine("[Serializable]");
                using (sourceWriter.WithNamedScope(
                           $"public struct {mergedStructDef.Name} : {mergedStructDef.InterfaceName}"))
                {
                    //Type enum definition
                    using (sourceWriter.WithNamedScope($"public enum {PolymorphicStructsConstants.TypeEnumName}"))
                    {
                        foreach (var s in structs)
                        {
                            sourceWriter.WriteLine($"{s.Name},");
                        }
                    }

                    //fields
                    {
                        sourceWriter.WriteField("public", TypeEnumName, TypeEnumFieldName);
                        foreach (var f in mergedFields)
                        {
                            sourceWriter.WriteField("public", f.Type.Name, f.FieldName);
                        }
                    }

                    //methods
                    {
                        foreach (var method in mergedStructDef.InterfaceMethods)
                        {
                            using (sourceWriter.WithMethodScope(method))
                            {
                                using (sourceWriter.WithNamedScope(
                                           $"switch ({TypeEnumFieldName})"))
                                {
                                    foreach (var s in structs)
                                    {
                                        using (sourceWriter.WithNamedScope(
                                                   $"case {TypeEnumName}.{s.Name}:"))
                                        {
                                            var variableName = $"instance_{s.Name}";

                                            sourceWriter.WriteLine($"var {variableName} = new {s.Name}(this);");

                                            if (method.ReturnsVoid)
                                            {
                                                sourceWriter.WriteLines(
                                                    $"{variableName}.{method.Name}({method.BuildParameterListForInvocation()});",
                                                    $"{variableName}.{s.ToMergedMethodName}(ref this);",
                                                    "break;"
                                                );
                                            }
                                            else
                                            {
                                                sourceWriter.WriteLines(
                                                    $"var r = {variableName}.{method.Name}({method.BuildParameterListForInvocation()});",
                                                    $"{variableName}.{s.ToMergedMethodName}(ref this);",
                                                    "return r;"
                                                );
                                            }
                                        }
                                    }

                                    using (sourceWriter.WithNamedScope("default:"))
                                    {
                                        sourceWriter.WriteLine(
                                            $"throw new System.ArgumentOutOfRangeException($\"Unexpected type id {{{PolymorphicStructsConstants.TypeEnumFieldName}}} for merged struct {mergedStructDef.Name}\");");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return sourceWriter.StringBuilder.ToString();
        }
    }
}