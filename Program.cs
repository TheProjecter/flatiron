using System;
using System.IO;

namespace Flatiron
{
    class Program
    {
        static FlatironEngine flatiron;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: flatiron <template dir> <output dir>");
                return;
            }

            flatiron = new FlatironEngine(args[0]);

            ProcessDirectory(args[0], args[1]);
        }

        static void ProcessDirectory(string srcDir, string destDir)
        {
            Console.WriteLine("Entering " + srcDir);

            foreach (var file in Directory.GetFiles(srcDir))
            {
                if (file.Contains(".tmpl.") || file.Contains(".inc.") || 
                    file.StartsWith(".") || (File.GetAttributes(file) & FileAttributes.Hidden) != 0)
                    continue;

                string destFile;

                if (file == "index.html")
                    destFile = Path.Combine(destDir, "index.html");
                else
                {
                    var dir = Path.Combine(destDir, file.Replace(".html", ""));
                    Directory.CreateDirectory(dir);
                    destFile = Path.Combine(dir, "index.html");
                }

                string destFileDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destFileDir))
                    Directory.CreateDirectory(destFileDir);

                if (File.Exists(destFile))
                    File.Delete(destFile);

                var result = flatiron.Evaluate(file);

                using (var w = new StreamWriter(File.OpenWrite(destFile)))
                {
                    w.Write(result);
                    w.Flush();
                }
            }

            Console.WriteLine("Leaving " + srcDir);

            foreach (var directory in Directory.GetDirectories(srcDir))
                if (!directory.StartsWith("."))
                    ProcessDirectory(directory, Path.Combine(destDir, directory));
        }
    }
}
