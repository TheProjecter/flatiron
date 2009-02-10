using Flatiron.Parsing;

namespace Flatiron.IronRuby
{
    class IronRubyCommandWriter : CommandWriter
    {
        string newline = "\r\n";

        protected override void WriteCode(string code)
        {
            Write(code);
            Write(newline);
        }

        protected override void WriteLiteral(string literal)
        {
            literal = literal.Replace("\"", "\\\"").Replace("\r\n", "\\r\\n").Replace("\r", "\\r").Replace("\n", "\\n");
            Write("scope.output.write(\"");
            Write(literal);
            Write("\")");
            Write(newline);
        }

        protected override void WriteExpression(string expression)
        {
            Write("scope.output.write(");
            Write(expression);
            Write(")");
            Write(newline);
        }

        protected override void WriteInclude(string expression)
        {
            Write("scope.include(");
            Write(expression);
            Write(")");
            Write(newline);
        }

        protected override void WriteSetParentTemplate(string expression)
        {
            Write("scope.set_parent_template(");
            Write(expression);
            Write(")");
            Write(newline);
        }

        protected override void WriteOutputToSection(string expression)
        {
            Write("scope.output_to_section(");
            Write(expression);
            Write(")");
            Write(newline);
        }

        protected override void WriteWriteChildSection(string expression)
        {
            Write("scope.write_child_section(");
            Write(expression);
            Write(")");
            Write(newline);
        }
    }
}
