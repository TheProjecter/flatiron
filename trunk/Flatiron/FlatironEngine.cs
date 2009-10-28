using System.Collections.Generic;
using System.IO;
using Flatiron.IronRuby;
using System;

namespace Flatiron
{
    public class FlatironEngine
    {
        public string TemplateRoot { get; set; }

        Dictionary<string, Template> templates;
        ILanguageSupport support;

        public FlatironEngine() : this(false) { }
        public FlatironEngine(bool debug) : this(Environment.CurrentDirectory, debug) { }
        public FlatironEngine(string templateRoot) : this(templateRoot, false) { }
        public FlatironEngine(string templateRoot, bool debug)
        {
            support = new IronRubySupport(debug);
            TemplateRoot = templateRoot;
        }

        #region Evaluate - Convenience overloads

        public string Evaluate(string template)
        {
            return Evaluate(ResolveTemplate(template, null));
        }

        public string Evaluate(string template, Dictionary<string, object> variables)
        {
            return Evaluate(ResolveTemplate(template, null), variables);
        }

        public string Evaluate(Template template)
        {
            return Evaluate(template, null);
        }

        #endregion

        public string Evaluate(Template template, Dictionary<string, object> variables)
        {
            using (var scope = new TemplateScope(this, template, variables))
            {
                EvaluateInternal(scope, null, null);
                return scope.GetContentsOfSection(TemplateScope.DefaultSection);
            }
        }

        /// <summary>
        /// Override to turn a string into a Template instance. Requester is non-null if we are resolving
        /// from a call to TemplateScope.SetParentTemplate/Include.
        /// Default implementation instantiates a new Template with a backing file relative to the requesting 
        /// Template's backing file, or relative to Root if requester is null.
        /// </summary>
        public virtual Template ResolveTemplate(string template, TemplateScope requester)
        {
            string relativeTo = requester == null ? TemplateRoot : Path.GetDirectoryName(requester.Template.BackingFile);
            return GetTemplate(Path.Combine(relativeTo, template));
        }

        internal Template GetTemplate(string templateFile)
        {
            if (templates == null)
                templates = new Dictionary<string, Template>();

            var fullName = Path.GetFullPath(templateFile);
            return templates.ContainsKey(fullName) ?
                templates[fullName] :
                (templates[fullName] = new Template(templateFile));
        }

        internal void EvaluateInternal(TemplateScope scope, TemplateScope includer, TemplateScope child)
        {
            Template template = scope.Template;

            if (template.NeedsParsing)
            {
                Console.WriteLine("reparsing " + template);
                template.Parse(support.CreateCommandWriter());
            }
            else Console.WriteLine("not reparsing " + template);


            // if we were included, we want to write to the includer's output.
            if (includer != null)
                scope.SetSectionWriter(TemplateScope.DefaultSection, includer.Output);

            support.Evaluate(scope);

            if (scope.Parent != null)
            {
                scope.Parent.Child = scope;
                EvaluateInternal(scope.Parent, null, scope);
                // set our output to whatever the parent's output was, since the parent is 'wrapping' us.
                scope.SetSectionWriter(TemplateScope.DefaultSection, scope.Parent.GetSectionWriter(TemplateScope.DefaultSection));
                scope.Parent.Child = null;
            }
        }
    }
}
