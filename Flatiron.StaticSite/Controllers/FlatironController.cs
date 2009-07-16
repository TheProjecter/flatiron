using System.IO;
using System.Reflection;
using Kayak.Framework;
using Kayak.Framework.IO;
using Kayak.Framework.Templating;
using System;

namespace Flatiron.StaticSite.Controllers
{
    public class FlatironController : KayakController
    {
        static TemplatingSerializer templatingSerializer;
        TemplateVariables template;

        TemplateVariables Template { get { return template ?? (template = new TemplateVariables()); } }

        protected override void AfterInvoke(MethodInfo method, ref object result)
        {
            if (template != null) result = template;
        }

        protected override ISerializer GetSerializer(MethodInfo method, object result)
        {
            return templatingSerializer ?? (templatingSerializer = new TemplatingSerializer("../../Views".Replace('/', Path.DirectorySeparatorChar)));
        }

        [MapPath("/")]
        [Template("index.html")]
        public void Root() { }

        [MapPath("/generate")]
        public void Generate(string source, string output, bool prettyUrls)
        {
            var flatiron = new FlatironEngine(true, source);
            source = CleanPath(source);
            output = CleanPath(output);
            ProcessDirectory(flatiron, source, output, prettyUrls);
            Response.Write("hooray!");
            Response.Complete();
        }

        string CleanPath(string path)
        {
            return path.Replace("file:///", "").Replace('/', Path.DirectorySeparatorChar);
        }

        void ProcessDirectory(FlatironEngine flatiron, string srcDir, string destDir, bool prettyUrls)
        {
            Console.WriteLine("Entering " + srcDir);

            foreach (var filePath in Directory.GetFiles(srcDir))
            {
                var fileName = Path.GetFileName(filePath);

                if (fileName.Contains(".prnt.") || fileName.Contains(".incl.") ||
                    fileName.StartsWith(".") || (File.GetAttributes(filePath) & FileAttributes.Hidden) != 0)
                    continue;

                string destFile;
                if (fileName == "index.html" || !prettyUrls)
                    destFile = Path.Combine(destDir, fileName);
                else
                {
                    var dir = Path.Combine(destDir, fileName.Replace(".html", ""));
                    Directory.CreateDirectory(dir);
                    destFile = Path.Combine(dir, "index.html");
                }

                string destFileDir = Path.GetDirectoryName(destFile);

                if (!Directory.Exists(destFileDir))
                    Directory.CreateDirectory(destFileDir);

                if (File.Exists(destFile))
                    File.Delete(destFile);

                Console.WriteLine(filePath + " -> ");

                string result;

                try
                {
                    result = flatiron.Evaluate(Path.GetFullPath(filePath));
                }
                catch (Exception e)
                {
                    var fed = FlatironExceptionData.GetInstance(e);
                    if (fed == null) throw new Exception("Error evaluating " + filePath + " (no FlatironExceptionData available)", e);
                    throw new Exception("Error evaluating " + fed.Template + " (line " + fed.Line + "): " + e.Message, e);
                }


                Console.WriteLine("    " + destFile);

                using (var w = new StreamWriter(File.OpenWrite(destFile)))
                {
                    w.Write(result);
                    w.Flush();
                }
            }

            Console.WriteLine("Leaving " + srcDir);

            foreach (var directory in Directory.GetDirectories(srcDir))
                if (!directory.StartsWith("."))
                {
                    var sepSplit = directory.Split(Path.DirectorySeparatorChar);
                    var dirName = sepSplit[sepSplit.Length - 1];
                    var dest = Path.Combine(destDir, dirName);
                    ProcessDirectory(flatiron, directory, dest, prettyUrls);
                }
        }
    }
}
