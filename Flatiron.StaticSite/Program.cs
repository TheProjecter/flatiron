using System;
using System.IO;
using Kayak.Framework;

namespace Flatiron
{
    class Program
    {
        public static void Main(string[] args)
        {
            var server = new KayakFrameworkServer();
            server.FindControllers();
            Console.WriteLine("Starting Flatiron server on port " + server.ListenEP.Port + "...");
            server.Start();
            Console.WriteLine("Server started, press enter to exit.");
            Console.ReadLine();
            server.Stop();
        }

        /*
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

        static 
         * */
    }
}
