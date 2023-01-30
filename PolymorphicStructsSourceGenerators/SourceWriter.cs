using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Core.SourceGen
{
    public enum MemberProtectionLevel
    {
        Internal,
        Public,
        Private,
        Protected,
    }

    public class SourceWriter
    {
        public StringBuilder StringBuilder = new StringBuilder();
        private int indentLevel = 0;

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

        public Scope WithScope()
        {
            return new Scope(this);
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

        public void WriteField(MemberProtectionLevel protectionLevel, string fieldType, string fieldName)
        {
            WriteLine($"{protectionLevel.ToString().ToLower()} {fieldType} {fieldName};");
        }

        public NamedScope WithNamedScope(string namedScopeLine)
        {
            return new NamedScope(this, namedScopeLine);
        }

        public struct Scope : IDisposable
        {
            public SourceWriter writer;

            public Scope(SourceWriter w)
            {
                writer = w;
                writer.WriteLine("{");
                writer.indentLevel++;
            }

            public void Dispose()
            {
                writer.indentLevel--;
                writer.WriteLine("}");
            }
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
            var returnType = method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString();
            var parameters = method.BuildParameterListForDeclaration();
            var line = $"public {returnType} {method.Name}({parameters})";
            return new NamedScope(this, line);
        }
    }
}