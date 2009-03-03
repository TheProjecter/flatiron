using System.Collections.Generic;
using System.IO;
using Flatiron.IronRuby;

namespace Flatiron
{
    public class FlatironEngine
    {
        public string TemplateRoot { get; set; }

        ILanguageSupport support;

        public FlatironEngine(bool debug)
        {
            support = new IronRubySupport(debug);
        }

        public FlatironEngine(bool debug, string templateRoot) : this(debug)
        {
            TemplateRoot = templateRoot;
        }

        public FlatironEngine() : this(false) { }
        public FlatironEngine(string templateRoot) : this(false, templateRoot) { }

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
        /// Override to turn a string passed to TemplateScope.SetParentTemplate/Include into a Template instance.
        /// Default implementation instantiates a new Template with a backing file relative to the requesting 
        /// Template's backing file, or relative to Root if requester is null. You might, for example, want to 
        /// cache Template instances so they don't have to be reparsed all the time.
        /// </summary>
        public virtual Template ResolveTemplate(string template, TemplateScope requester)
        {
            string relativeTo = requester == null ? TemplateRoot : Path.GetDirectoryName(requester.Template.BackingFile);
            return new Template(Path.Combine(relativeTo, template));
        }

        internal void EvaluateInternal(TemplateScope scope, TemplateScope includer, TemplateScope child)
        {
            Template template = scope.Template;

            if (template.NeedsParsing)
                template.Parse(support.CreateCommandWriter());

            // if we were included, we want to write to the includer's output.
            if (includer != null) scope.SetSectionWriter(TemplateScope.DefaultSection, includer.Output);

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
