using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Core.SourceGen
{
    public class SourceWriter
    {
        public StringBuilder StringBuilder;
        private int indentLevel = 0;

        public SourceWriter()
        {
            StringBuilder = new StringBuilder();
            WriteLine(@"/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
");
        }

        public void WriteLines(params string[] lines)
        {
            foreach (var l in lines)
            {
                WriteLine(l);
            }
        }
        
        public void WriteLine(string line)
        {
            StringBuilder.Append(GetIndentString());
            StringBuilder.Append(line);
            StringBuilder.Append('\n');
        }

        private string GetIndentString()
        {
            var indent = "";
            for (var i = 0; i < indentLevel; i++)
            {
                indent += "\t";
            }

            return indent;
        }

        public NamedScope WithNamespace(string namespaceName)
        {
            if (!string.IsNullOrEmpty(namespaceName) && !string.IsNullOrWhiteSpace(namespaceName))
            {
                return new NamedScope(this, $"namespace {namespaceName}");
            }
            else
            {
                return new NamedScope(this, "");
            }
        }

        public NamedScope WithNamedScope(string namedScopeLine)
        {
            return new NamedScope(this, namedScopeLine);
        }

        public struct NamedScope : IDisposable
        {
            private SourceWriter writer;
            private string namedScopeLine;

            public NamedScope(SourceWriter writer, string namedScopeLine)
            {
                this.writer = writer;
                this.namedScopeLine = namedScopeLine;
                if (!string.IsNullOrEmpty(namedScopeLine))
                {
                    writer.WriteLine(namedScopeLine);
                    writer.WriteLine("{");
                    writer.indentLevel++;
                }
            }

            public void Dispose()
            {
                if (!string.IsNullOrEmpty(namedScopeLine))
                {
                    writer.indentLevel--;
                    writer.WriteLine("}");
                }
            }
        }

        public void WriteUsings(IEnumerable<string> usingDirectives)
        {
            foreach (var usingDirective in usingDirectives)
            {
                if (!string.IsNullOrEmpty(usingDirective) && !string.IsNullOrWhiteSpace(usingDirective))
                {
                    WriteLine($"using {usingDirective};");
                }
            }
        }

        public IDisposable WithMethodScope(IMethodSymbol method)
        {
            return WithNamedScope(
                $"{method.DeclaredAccessibility.AccessibilityToString()} {method.ReturnType} {method.Name}({method.BuildParameterListForDeclaration()})");
        }

        public void WriteField(string accessibility, string fieldType, string fieldName)
        {
            WriteLine($"{accessibility} {fieldType} {fieldName};");
        }
    }
}