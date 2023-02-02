using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Core.SourceGen
{
    public class SourceWriter
    {
        public StringBuilder StringBuilder;
        private int indentLevel;
        private readonly string lineEnding;
        private string indent;

        private int IndentLevel
        {
            get => indentLevel;
            set
            {
                if (indentLevel != value)
                {
                    indentLevel = value;
                    indent = "";
                    for (var i = 0; i < IndentLevel; i++)
                    {
                        indent += "\t";
                    }
                }
            }
        }

        public SourceWriter()
        {
            StringBuilder = new StringBuilder();
            WriteLine(@"/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/");

            lineEnding = "\r\n";
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
            StringBuilder.Append(indent);
            StringBuilder.Append(line);
            StringBuilder.Append(lineEnding);
        }

        public NamedScope WithTypeScope(string typeDeclaration)
        {
            WriteLine("");
            return WithNamedScope(typeDeclaration);
        }

        public NamedScope WithMethodScope(string methodDeclaration)
        {
            WriteLine("");
            return WithNamedScope(methodDeclaration);
        }
        
        public NamedScope WithNamespace(string namespaceName)
        {
            if (!string.IsNullOrEmpty(namespaceName) && !string.IsNullOrWhiteSpace(namespaceName))
            {
                WriteLine("");
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
                    writer.IndentLevel++;
                }
            }

            public void Dispose()
            {
                if (!string.IsNullOrEmpty(namedScopeLine))
                {
                    writer.IndentLevel--;
                    writer.WriteLine("}");
                }
            }
        }

        public void WriteUsings(IEnumerable<string> usingDirectives)
        {
            WriteLine("");
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
            return WithMethodScope(
                $"{method.DeclaredAccessibility.AccessibilityToString()} {method.ReturnType} {method.Name}({method.BuildParameterListForDeclaration()})");
        }

        public void WriteField(string accessibility, string fieldType, string fieldName)
        {
            WriteLine($"{accessibility} {fieldType} {fieldName};");
        }
    }
}