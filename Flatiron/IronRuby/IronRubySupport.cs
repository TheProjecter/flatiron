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
        ScriptRuntime runtime;
        ScriptEngine rubyEngine;

        bool debug;

        public IronRubySupport(bool debug)
        {
            this.debug = debug;

            ScriptRuntimeSetup runtimeSetup = new ScriptRuntimeSetup();
            LanguageSetup rubySetup = Ruby.CreateRubySetup();

            if (debug)
            {
                runtimeSetup.DebugMode = true;
                rubySetup.ExceptionDetail = true;
            }

            runtimeSetup.LanguageSetups.Add(rubySetup);

            runtime = new ScriptRuntime(runtimeSetup); // can in theory be kept around
            rubyEngine = runtime.GetEngine("ruby"); // so can this
        }

        public CommandWriter CreateCommandWriter()
        {
            return new IronRubyCommandWriter();
        }

        public void Evaluate(TemplateScope scope)
        {
            var executable = scope.Template.Executable;
            ScriptSource source;

            if (debug)
                // have to pass in this bogus file name so that IronRuby will give us line numbers 
                // in the backtrace when exceptions occur at runtime. stupid!
                source = rubyEngine.CreateScriptSourceFromString(executable, "omg_rly.rb", SourceCodeKind.File);
            else
                source = rubyEngine.CreateScriptSourceFromString(executable, SourceCodeKind.Statements);
            
            
            // for some reason the engine keeps track of these instances (wtf)...
            ScriptScope scriptScope = runtime.CreateScope();
            // ...so we have to unset this later.
            scriptScope.SetVariable("scope", scope);


            if (scope.Variables != null) 
                foreach (var pair in scope.Variables)
                    scriptScope.SetVariable(pair.Key, pair.Value);

            try
            {
                // latest DLR/IronRuby breaks the executing of compiled code somehow.
                //if (!(scope.Template.CompiledExecutable is CompiledCode))
                //    scope.Template.CompiledExecutable = source.Compile();
                //((CompiledCode)scope.Template.CompiledExecutable).Execute(scriptScope);

                source.Execute(scriptScope);
                
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
                // first line of backtrace is "<bogus file name>:<line number>" (no longer works with DLR .91)
                // int.TryParse(red.Backtrace[0].ToString().Split(':')[1], out line);
                // return line;
                return -1;
            }
            else return -1;
        }
    }
}