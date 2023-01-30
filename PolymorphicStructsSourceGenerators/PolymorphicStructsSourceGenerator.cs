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
            SourceGenUtils.AttachDebugger();
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
            var baseStructDef = BuildBaseStructForInterface(context, interfaceDeclarationSyntax);
            var structImplDefs = BuildStructImplDefs(context, syntaxReceiver, baseStructDef);
            if (structImplDefs.Count == 0)
            {
                return;
            }

            var mergedFields = BuildMergedStructFields(structImplDefs);

            var baseStructSource = GenerateBaseStructSource(baseStructDef, structImplDefs, mergedFields);
            context.AddSource($"{baseStructDef.Name}.gen.cs", baseStructSource);

            foreach (var s in structImplDefs)
            {
                var implStructSource = GenerateImplStructSource(baseStructDef, s, mergedFields);
                context.AddSource($"{s.Name}.gen.cs", implStructSource);
            }
        }

        private string GenerateImplStructSource(BaseStructDef baseStructDef, StructImplDef structImplDef,
            List<MergedStructField> mergedFields)
        {
            var sourceWriter = new SourceWriter();
            using (sourceWriter.WithNamespace(structImplDef.Namespace))
            {
                using (sourceWriter.WithNamedScope($"public partial struct {structImplDef.Name}"))
                {
                    //From merge struct (constructor)
                    using (sourceWriter.WithNamedScope($"public {structImplDef.Name}({baseStructDef.Name} s)"))
                    {
                        foreach (var f in structImplDef.Fields)
                        {
                            sourceWriter.WriteLine($"{f.FieldName} = s.{f.BaseStructFieldName};");
                        }
                    }

                    //To base struct (by ref)
                    var toMergedStructMethodName = $"To{baseStructDef.Name}";
                    using (sourceWriter.WithNamedScope(
                               $"public void {toMergedStructMethodName}(ref {baseStructDef.Name} s)"))
                    {
                        sourceWriter.WriteLine(
                            $"s.{PolymorphicStructsConstants.TypeEnumFieldName} = {baseStructDef.Name}.{PolymorphicStructsConstants.TypeEnumName}.{structImplDef.Name};");
                        foreach (var mergedField in mergedFields)
                        {
                            if (mergedField.StructToFieldName.TryGetValue(structImplDef, out var fieldNameInStruct))
                            {
                                sourceWriter.WriteLine($"s.{mergedField.FieldName} = {fieldNameInStruct};");
                            }
                        }
                    }
                    
                    //To Base struct
                    using (sourceWriter.WithNamedScope($"public {baseStructDef.Name} To{baseStructDef.Name}()"))
                    {
                        sourceWriter.WriteLine($"var s = new {baseStructDef.Name}();");
                        sourceWriter.WriteLine($"{toMergedStructMethodName}(ref s);");
                        sourceWriter.WriteLine("return s;");
                    }
                }
            }

            return sourceWriter.StringBuilder.ToString();
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

        private string GenerateBaseStructSource(BaseStructDef baseStructDef, List<StructImplDef> structs,
            List<MergedStructField> mergedFields)
        {
            var sourceWriter = new SourceWriter();
            sourceWriter.WriteUsings(baseStructDef.UsingDirectives);
            using (sourceWriter.WithNamespace(baseStructDef.Namespace))
            {
                //Base struct definition
                sourceWriter.WriteLine("[Serializable]");
                using (sourceWriter.WithNamedScope($"public struct {baseStructDef.Name}"))
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
                        sourceWriter.WriteField(MemberProtectionLevel.Public, PolymorphicStructsConstants.TypeEnumName,
                            PolymorphicStructsConstants.TypeEnumFieldName);
                        foreach (var f in mergedFields)
                        {
                            sourceWriter.WriteField(MemberProtectionLevel.Public, f.Type.Name, f.FieldName);
                        }
                    }

                    //methods
                    {
                        foreach (var method in baseStructDef.InterfaceMethods)
                        {
                            using (sourceWriter.WithMethodScope(method))
                            {
                                sourceWriter.WriteLine("throw new Exception();");
                            }
                        }
                    }
                }
            }

            return sourceWriter.StringBuilder.ToString();
        }
    }
}