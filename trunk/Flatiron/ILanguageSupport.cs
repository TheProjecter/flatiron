using Flatiron.Parsing;

namespace Flatiron
{
    public interface ILanguageSupport
    {
        /// <summary>
        /// This method does the hard work. The scope represents implicit variables and functions available during the
        /// "execution" of a template. It has a Template property, which has an Executable property (which was created by
        /// a CommandWriter this ILanguageSupport returned from CreateCommandWriter()). You are supposed to be executing 
        /// that Executable 'in' this scope. Should be thread-safe.
        /// </summary>
        void Evaluate(TemplateScope scope);

        /// <summary>
        /// Create a CommandWriter to write commands in a language this instance can evaluate. 
        /// Called after a template is parsed into TemplateCommands.
        /// </summary>
        CommandWriter CreateCommandWriter();
    }
}
