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
        [ThreadStatic]
        static ScriptEngine engine; // temporary hack! 

        bool debug;

        public IronRubySupport(bool debug)
        {
            this.debug = debug;
        }

        public CommandWriter CreateCommandWriter()
        {
            return new IronRubyCommandWriter();
        }

        public void Evaluate(TemplateScope scope)
        {
            if (engine == null)
            {
                // temporary hack! how do these objects (ScriptRuntime, ScriptEngine) actually work??

                ScriptRuntimeSetup runtimeSetup = new ScriptRuntimeSetup();
                LanguageSetup rubySetup = Ruby.CreateLanguageSetup();

                if (debug)
                {
                    runtimeSetup.DebugMode = true;
                    rubySetup.ExceptionDetail = true;
                }

                runtimeSetup.LanguageSetups.Add(rubySetup);

                engine = new ScriptRuntime(runtimeSetup).GetEngine("ruby");
            }

            // for some reason the engine keeps track of these instances (wtf)...
            ScriptScope scriptScope = engine.CreateScope();
            // ...so we have to unset this later.
            scriptScope.SetVariable("scope", scope);


            if (scope.Variables != null) 
                foreach (var key in scope.Variables.Keys)
                    scriptScope.SetVariable(key, scope.Variables[key]);

            var executable = scope.Template.Executable;
            
            ScriptSource source;
            if (debug)
                // have to pass in this bogus file name so that IronRuby will give us line numbers 
                // in the backtrace when exceptions occur at runtime. stupid!
                source = engine.CreateScriptSourceFromString(executable, "omg_rly.rb", SourceCodeKind.File);
            else
                source = engine.CreateScriptSourceFromString(executable, SourceCodeKind.Statements);

            if(!(scope.Template.CompiledExecutable is CompiledCode))
                scope.Template.CompiledExecutable = source.Compile();

            try
            {
                ((CompiledCode)scope.Template.CompiledExecutable).Execute(scriptScope);
            }
            catch (Exception e)
            {
                // need to worry about ThreadAbortException?
                FlatironExceptionData.AssociateInstance(e, scope.Template, GetExecutableLine(e));
                throw;
            }
            finally
            {
                scriptScope.RemoveVariable("scope");
            }
        }

        int GetExecutableLine(Exception e)
        {
            if (e is SyntaxErrorException)
                return (e as SyntaxErrorException).Line;
            else if (debug)
            {
                RubyExceptionData red = RubyExceptionData.GetInstance(e);
                int line = 0;
                // first line of backtrace is "<bogus file name>:<line number>"
                int.TryParse(red.Backtrace[0].ToString().Split(':')[1], out line);
                return line;
            }
            else return -1;
        }
    }
}