using System;
using Flatiron.Parsing;
using IronRuby;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Threading;

namespace Flatiron.IronRuby
{
    class IronRubySupport : ILanguageSupport
    {
        public CommandWriter CreateCommandWriter()
        {
            return new IronRubyCommandWriter();
        }

        public void Evaluate(TemplateScope scope)
        {
            // TODO: keep around what can be kept around to make this process
            // more efficient
            ScriptRuntimeSetup runtimeSetup = new ScriptRuntimeSetup();
            runtimeSetup.DebugMode = true;
            LanguageSetup rubySetup = Ruby.CreateRubySetup();
            rubySetup.ExceptionDetail = true;
            runtimeSetup.LanguageSetups.Add(rubySetup);
            ScriptRuntime runtime = new ScriptRuntime(runtimeSetup);
            ScriptEngine engine = runtime.GetRubyEngine();

            ScriptScope scriptScope = engine.CreateScope();
            
            scriptScope.SetVariable("scope", scope);


            if (scope.Variables != null) 
                foreach (var key in scope.Variables.Keys)
                    scriptScope.SetVariable(key, scope.Variables[key]);

            var executable = scope.Template.Executable;

            // have to pass in this bogus file name so that IronRuby will give us line numbers 
            // in the backtrace when exceptions occur at runtime. stupid!
            ScriptSource source = engine.CreateScriptSourceFromString(executable, "omg_rly.rb", SourceCodeKind.File);

            // want to cache this...
            //CompiledCode c = source.Compile();

            try
            {
                // ...and do this:
                //c.Execute(scope);
                source.Execute(scriptScope);
            }
            catch (Exception e)
            {
                // need to worry about ThreadAbortException?
                FlatironExceptionData.AssociateInstance(e, scope.Template, GetExecutableLine(e));
                throw;
            }       
        }

        int GetExecutableLine(Exception e)
        {
            if (e is SyntaxErrorException)
                return (e as SyntaxErrorException).Line;
            else
            {
                RubyExceptionData red = RubyExceptionData.GetInstance(e);
                int line = 0;
                // first line of backtrace is "<bogus file name>:<line number>"
                int.TryParse(red.Backtrace[0].ToString().Split(':')[1], out line);
                return line;
            }
        }
    }
}