﻿using System.Collections.Generic;
using System.IO;
using Flatiron.IronRuby;

namespace Flatiron
{
    public class Flatiron
    {
        public DirectoryInfo TemplateRoot { get; set; }

        ILanguageSupport support;

        public Flatiron()
        {
            support = new IronRubySupport();
        }

        public Flatiron(DirectoryInfo templateRoot) : this()
        {
            TemplateRoot = templateRoot;
        }

        public string Evaluate(Template template, Dictionary<string, object> variables)
        {
            var scope = new TemplateScope(this, template, variables);
            EvaluateInternal(scope, null, null);
            return scope.GetContentsOfSection(TemplateScope.DefaultSection);
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

        /// <summary>
        /// Override to turn a string passed to TemplateScope.SetParentTemplate/Include into a Template instance.
        /// Default implementation instantiates a new Template with a backing file relative to the requesting Template's backing file,
        /// or relative to Root if requester is null.
        /// You might, for example, want to cache Template instances so they don't have to be reparsed all the time.
        /// </summary>
        public virtual Template ResolveTemplate(string template, TemplateScope requester)
        {
            string relativeTo = requester == null ? TemplateRoot.FullName : requester.Template.BackingFile.DirectoryName;
            return new Template(new FileInfo(Path.Combine(relativeTo, template)));
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
            }
        }
    }
}