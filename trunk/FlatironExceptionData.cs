using System;
using System.Collections.Generic;

namespace Flatiron
{
    
    [Serializable] // make the Exception.Data dictionary happy
    public class FlatironExceptionData
    {
        // nice pattern RubyExceptionData!
        static readonly object _dataKey = new object();

        /// <summary>
        /// The template that was being evaluated when the error occurred.
        /// </summary>
        public Template Template { get; private set; }

        /// <summary>
        /// The line in the user's template file that caused the error.
        /// </summary>
        public int Line { get; private set; }

        private FlatironExceptionData(Template template, int executableLine)
        {
            Template = template;
            Line = Template.ExecutableToTemplateLine(executableLine);
        }

        public static void AssociateInstance(Exception exception, Template template, int executableLine)
        {
            exception.Data[_dataKey] = new FlatironExceptionData(template, executableLine);
        }

        public static FlatironExceptionData GetInstance(Exception e)
        {
            return e.Data.Contains(_dataKey) ? e.Data[_dataKey] as FlatironExceptionData : null;
        }
    }
}
