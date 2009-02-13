using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flatiron
{
    /// <summary>
    /// Represents a request to render a template. Handles includes and parenting.
    /// </summary>
    public class TemplateScope
    {
        public static readonly string DefaultSection = "__default";

        FlatironEngine flatiron;
        Dictionary<string, StringWriter> sectionWriters; // allow templates to write to named sections

        /// <summary>
        ///  Gets the template this scope is evaluating.
        /// </summary>
        public Template Template { get; private set; }

        /// <summary>
        /// Name-value pairs visible in this scope.
        /// </summary>
        public Dictionary<string, object> Variables { get; private set; }
        
        /// <summary>
        /// Gets the StringWriter for the current section.
        /// </summary>
        public StringWriter Output { get; private set; }

        /// <summary>
        /// Some arbitrary state to play with.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the parent scope (or null if SetParentTemplate(file) has not populated it).
        /// </summary>
        public TemplateScope Parent { get; private set; }

        /// <summary>
        /// Get the child scope (or null if this scope is not the parent of another scope).
        /// </summary>
        public TemplateScope Child { get; internal set; }

        internal TemplateScope(FlatironEngine flatiron, Template template, Dictionary<string, object> variables)
        {
            this.flatiron = flatiron;
            Template = template;
            Variables = variables;
            sectionWriters = new Dictionary<string,StringWriter>();
            OutputToSection(DefaultSection);
        }

        internal string GetContentsOfSection(string section)
        {
            StringWriter w = GetSectionWriter(section);
            return w == null ? null : w.GetStringBuilder().ToString();
        }

        internal StringWriter GetSectionWriter(string section)
        {
            return sectionWriters.ContainsKey(section) ? sectionWriters[section] : null;
        }

        internal void SetSectionWriter(string section, StringWriter writer)
        {
            if (Output == sectionWriters[section])
                Output = writer;

            sectionWriters[section] = writer;
        }

        #region Template directive support

        /// <summary>
        /// Swizzle the Output property so that it writes to the named section.
        /// </summary>
        protected void OutputToSection(string section)
        {
            if (!sectionWriters.ContainsKey(section))
                sectionWriters[section] = new StringWriter(new StringBuilder());

            Output = sectionWriters[section];
        }


        /// <summary>
        /// Write the contents of the named section of the child scope.
        /// </summary>
        /// <param name="section"></param>
        protected void WriteChildSection(string section)
        {
            if (Child == null)
                throw new Exception("Cannot write section '" + section + "' because this scope is not the parent of another scope.");

            Output.Write(Child.GetContentsOfSection(section) ?? "");
        }

        /// <summary>
        /// Evaluate the named template and write its result to Output.
        /// </summary>
        protected void Include(string file)
        {
            Template include = flatiron.ResolveTemplate(file, this);

            if (include == null)
                throw new Exception("Could not find file for inclusion: " + file);

            TemplateScope includeScope = new TemplateScope(flatiron, include, Variables);
            flatiron.EvaluateInternal(includeScope, this, null);
        }

        /// <summary>
        /// Set the parent template of this scope.
        /// </summary>
        protected void SetParentTemplate(string file)
        {
            Template parent = flatiron.ResolveTemplate(file, this);

            if (parent == null)
                Parent = null;
            else if (parent == Template) 
                // i'm a little iffy on this check. there may be legitimate times when you want to 
                // do this, but if i was to let you, you could cause gnarly stack overflows and be 
                // confused as to why that was happening... so no recursion support for now.
                throw new Exception("Scope's parent template cannot be the same as the scope's template (\"no recursion for you!\")");
            else
                Parent = new TemplateScope(flatiron, parent, Variables);
        }

        #endregion
    }
}
