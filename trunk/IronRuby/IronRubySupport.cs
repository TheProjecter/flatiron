using System;
using Flatiron.Parsing;
using IronRuby;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

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

            ScriptSource source = engine.CreateScriptSourceFromString(executable);

            // want to cache this...
            //CompiledCode c = source.Compile();

            try
            {
                // ...and do this:
                //c.Execute(scope);
                source.Execute(scriptScope);
            }
            catch (SyntaxErrorException e)
            {
                throw new TemplateEvaluationException(scope.Template, e.Line, e);
            }
            catch (Exception e)
            {
                // TODO: extract line numbers from runtime exceptions
                // currently no one knows how: http://www.ruby-forum.com/topic/173123
                // seriously? wtf IronRuby...

                //RubyExceptionData red = RubyExceptionData.GetInstance(e);

                //foreach (object o in red.Backtrace) 
                //    Console.WriteLine(o);

                //Console.WriteLine("Error! " + red.Message);

                throw new Exception("Error while evaluating " + scope.Template, e);
            }       
        }
    }
}