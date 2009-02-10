using System;
using System.Collections.Generic;
using System.IO;
using Flatiron.Parsing;

namespace Flatiron
{
    /// <summary>
    /// Represents a template file. Maps lines in the Executable source to the BackingFile. Thread-safe.
    /// </summary>
    public class Template
    {
        public FileInfo BackingFile { get; private set; }
        public string Executable { get; private set; }
        public bool NeedsParsing { get { lock (this) return lastModified != BackingFile.LastWriteTime; } }

        DateTime lastModified;
        TemplateCommand[] commands;

        public Template(FileInfo file)
        {
            BackingFile = file;
        }

        public void Parse(CommandWriter writer)
        {
            lock (this)
            {
                List<TemplateCommand> cmds = new List<TemplateCommand>();

                using (CommandReader reader = new CommandReader(BackingFile.OpenRead()))
                {
                    TemplateCommand cmd;
                    while ((cmd = reader.ReadCommand()) != null)
                        cmds.Add(cmd);
                }

                foreach (TemplateCommand cmd in cmds)
                    writer.WriteCommand(cmd);

                commands = cmds.ToArray();

                Executable = writer.Executable;

                lastModified = BackingFile.LastWriteTime;
            }
        }

        /// <summary>
        /// Convert a line number in the executable template code to a line number in the user's template file.
        /// </summary>
        public int ExecutableToTemplateLine(int execLine)
        {
            int execLines = 0;
            int trouble = 0;

            for(int i = 0; i < commands.Length; i++)
            {
                TemplateCommand cmd = commands[i];

                execLines += cmd.ExecutableLines;

                if (execLines >= execLine)
                {
                    trouble = i;
                    break;
                }
            }

            int tempLines = 0;

            for (int i = 0; i <= trouble; i++)
            {
                TemplateCommand cmd = commands[i];
                tempLines += cmd.TemplateLines;
            }

            return tempLines += execLine - execLines;
        }

        public override string ToString()
        {
            return "[" + BackingFile + "]";
        }
    }
}