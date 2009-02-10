using System;
using System.IO;

namespace Flatiron
{
    class Program
    {
        static Flatiron flatiron;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: flatiron <template dir> <output dir>");
                return;
            }

            flatiron = new Flatiron(new DirectoryInfo(args[0]));

            ProcessDirectory(new DirectoryInfo(args[0]), new DirectoryInfo(args[1]));
        }

        static void ProcessDirectory(DirectoryInfo srcDir, DirectoryInfo destDir)
        {
            Console.WriteLine("Entering " + srcDir);

            foreach (var file in srcDir.GetFiles())
            {
                if (file.Name.Contains(".tmpl.") || file.Name.Contains(".inc.") || 
                    file.Name.StartsWith(".") || (file.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                FileInfo destFile;

                if (file.Name == "index.html")
                    destFile = new FileInfo(Path.Combine(destDir.FullName, "index.html"));
                else
                {
                    var dir = Path.Combine(destDir.FullName, file.Name.Replace(".html", ""));
                    Directory.CreateDirectory(dir);
                    destFile = new FileInfo(Path.Combine(dir, "index.html"));
                }

                if (!destFile.Directory.Exists)
                    destFile.Directory.Create();

                if (destFile.Exists)
                    destFile.Delete();

                var result = flatiron.Evaluate(file.FullName);

                using (var w = new StreamWriter(destFile.OpenWrite()))
                {
                    w.Write(result);
                    w.Flush();
                }
            }

            Console.WriteLine("Leaving " + srcDir);

            foreach (var directory in srcDir.GetDirectories())
                if (!directory.Name.StartsWith("."))
                    ProcessDirectory(directory, new DirectoryInfo(Path.Combine(destDir.FullName, directory.Name)));
        }
    }
}
