using System.Text;

namespace Flatiron.Parsing
{
    /// <summary>
    /// Writes executable commands in a language which can be evaluated by an implementer of ILanguageSupport.
    /// Counts how many lines are produced doing so. All abstract methods should be implemented in terms of Write(string).
    /// Not thread-safe! Thrown away after use.
    /// </summary>
    public abstract class CommandWriter
    {
        public string Executable { get { return sb.ToString(); } }

        int lines;
        StringBuilder sb;

        public CommandWriter()
        {
            sb = new StringBuilder();
        }

        internal void WriteCommand(TemplateCommand cmd)
        {
            lines = 0;
            switch (cmd.Type)
            {
                case TemplateCommandType.Code:
                    WriteCode(cmd.Body);
                    break;
                case TemplateCommandType.Literal:
                    WriteLiteral(cmd.Body);
                    break;
                case TemplateCommandType.Expression:
                    WriteExpression(cmd.Body);
                    break;
                case TemplateCommandType.Include:
                    WriteInclude(cmd.Body);
                    break;
                case TemplateCommandType.SetParentTemplate:
                    WriteSetParentTemplate(cmd.Body);
                    break;
                case TemplateCommandType.OutputToSection:
                    cmd.Body = cmd.Body.Trim();
                    WriteOutputToSection(string.IsNullOrEmpty(cmd.Body) ? null : cmd.Body);
                    break;
                case TemplateCommandType.WriteChildSection:
                    cmd.Body = cmd.Body.Trim();
                    WriteWriteChildSection(string.IsNullOrEmpty(cmd.Body) ? null : cmd.Body);
                    break;
            }
            cmd.ExecutableLines = lines;
        }

        protected void Write(string str)
        {
            lines += str.GetNumLines();
            sb.Append(str);
        }

        /// <summary>
        /// Write code. Likely that no transformations will be required
        /// </summary>
        protected abstract void WriteCode(string code);

        /// <summary>
        /// Write a call to the Write method of the scope's Output property with the string as a string literal argument. 
        /// You'll probably need to escape newlines and characters that terminate a string in your language.
        /// </summary>
        protected abstract void WriteLiteral(string literal);

        /// <summary>
        /// Write a call to the Write method of the scope's Output property with the expression as an argument.
        /// </summary>
        protected abstract void WriteExpression(string expression);

        /// <summary>
        /// Write a call to the scope's Include method with the expression as an argument.
        /// </summary>
        protected abstract void WriteInclude(string expression);

        /// <summary>
        /// Write a call to the scope's SetParentTemplate method with the expression as an argument.
        /// </summary>
        protected abstract void WriteSetParentTemplate(string expression);

        /// <summary>
        /// Write a call to the scope's OutputToSection method with the expression as an argument,
        /// or TemplateScope.DefaultSection if expression is null.
        /// </summary>
        protected abstract void WriteOutputToSection(string expression);

        /// <summary>
        /// Write a call to the scope's WriteChildSection method with the expression as an argument,
        /// or TemplateScope.DefaultSection if expression is null.
        /// </summary>
        protected abstract void WriteWriteChildSection(string expression);
    }
}
