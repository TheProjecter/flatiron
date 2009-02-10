using System;

namespace Flatiron
{
    public class TemplateEvaluationException : Exception
    {
        /// <summary>
        /// The template that was being evaluated when the error occurred.
        /// </summary>
        public Template Template { get; private set; }

        /// <summary>
        /// The line in the user's template file that caused the error.
        /// </summary>
        public int Line { get; private set; }

        public override string Message
        {
            get { return "Error evaluating template " + Template + " on line " + Line; }
        }

        public TemplateEvaluationException(Template template, int executableLine, Exception inner)
            : base(null, inner)
        {
            Template = template;
            Line = Template.ExecutableToTemplateLine(executableLine);
        }
    }
}
