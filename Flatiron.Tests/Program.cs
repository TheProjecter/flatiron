using System;
using IronRuby;
using Microsoft.Scripting.Hosting;

namespace Flatiron.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // arrg. ironruby leaks memory, ruining my whole project. dammit ironruby!
            ScriptRuntimeSetup runtimeSetup = new ScriptRuntimeSetup();
            LanguageSetup rubySetup = Ruby.CreateLanguageSetup();
            runtimeSetup.LanguageSetups.Add(rubySetup);
            var runtime = new ScriptRuntime(runtimeSetup);

            while (Console.ReadLine() != "q")
            {
                ScriptEngine engine = runtime.GetEngine("ruby");

                ScriptScope scriptScope = engine.CreateScope();
                //scriptScope.SetVariable("leak", new LeakedObject());
                scriptScope.SetVariable("console", Console.Out);

                ScriptSource scriptSource = engine.CreateScriptSourceFromString("tag = {};\ntag['key'] = 'value';\nconsole.write_line(tag['key']);");
                
                scriptSource.Execute(scriptScope);

                foreach (var name in scriptScope.GetVariableNames())
                    scriptScope.RemoveVariable(name);


                GC.Collect();
                Console.WriteLine("Memory usage is " + (GC.GetTotalMemory(true) / 1024.0) + "kb");
            }
        }

        public class LeakedObject
        {
            public void Speak()
            {
                Console.WriteLine("I'm a leaked object.");
            }
        }
    }
}
