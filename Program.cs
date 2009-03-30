using System;
using System.IO;

namespace Flatiron
{
    class Program
    {
        static FlatironEngine flatiron;

        public static void Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("usage: flatiron [--pretty-urls] <template dir> <output dir>");
                return;
            }
            bool prettyUrls = false;

            if (args[0] == "--pretty-urls")
            {
                prettyUrls = true;
                args = new string[] { args[1], args[2] };
            }

            flatiron = new FlatironEngine(true, args[0]);

            ProcessDirectory(args[0], args[1], prettyUrls);
        }

        static void ProcessDirectory(string srcDir, string destDir, bool prettyUrls)
        {
            Console.WriteLine("Entering " + srcDir);

            foreach (var filePath in Directory.GetFiles(srcDir))
            {
                var fileName = Path.GetFileName(filePath);

                if (fileName.Contains(".tmpl.") || fileName.Contains(".inc.") || 
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
                    result = flatiron.Evaluate(fileName);
                }
                catch (Exception e)
                {
                    var fed = FlatironExceptionData.GetInstance(e);
                    if (fed == null) throw;
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
                    ProcessDirectory(directory, Path.Combine(destDir, directory), prettyUrls);
        }
    }
}
