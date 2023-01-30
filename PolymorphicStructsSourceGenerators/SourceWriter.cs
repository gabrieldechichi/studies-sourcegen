using System;
using System.Collections.Generic;
using System.Text;

namespace Core.SourceGen
{
    
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

        public NamespaceScope WithNamespace(string namespaceName)
        {
            return new NamespaceScope(this, namespaceName);
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

        public struct NamespaceScope : IDisposable
        {
            private SourceWriter writer;
            private string namespaceName;

            public NamespaceScope(SourceWriter writer, string namespaceName)
            {
                this.writer = writer;
                this.namespaceName = namespaceName;
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    writer.WriteLine("namespace " + namespaceName);
                    writer.WriteLine("{");
                    writer.indentLevel++;
                }
            }
            public void Dispose()
            {
                if (!string.IsNullOrEmpty(namespaceName))
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
    }
}