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
        public string BackingFile { get; private set; }
        public string Executable { get; private set; }
        public object CompiledExecutable { get; set; }
        public bool NeedsParsing { get { lock (this) return lastModified != File.GetLastWriteTime(BackingFile); } }

        DateTime lastModified;
        TemplateCommand[] commands;

        public Template(string backingFile)
        {
            BackingFile = backingFile;
        }

        public void Parse(CommandWriter writer)
        {
            lock (this)
            {
                List<TemplateCommand> cmds = new List<TemplateCommand>();

                using (CommandReader reader = new CommandReader(File.OpenRead(BackingFile)))
                {
                    TemplateCommand cmd;
                    while ((cmd = reader.ReadCommand()) != null)
                        cmds.Add(cmd);
                }

                foreach (TemplateCommand cmd in cmds)
                    writer.WriteCommand(cmd);

                commands = cmds.ToArray();

                Executable = writer.Executable;

                lastModified = File.GetLastWriteTime(BackingFile);
            }
        }

        /// <summary>
        /// Convert a line number in the executable template code to a line number in the user's template file.
        /// </summary>
        public int ExecutableToTemplateLine(int line)
        {
            int indexOfCommand = 0; // the command corresponding to 'line'
            int executableLines = 0;

            for(int i = 0; i < commands.Length; i++)
            {
                TemplateCommand cmd = commands[i];

                // accumulate the number of lines commands take up
                executableLines += cmd.ExecutableLines;

                // if we've reached the line in question, we're done
                if (executableLines >= line)
                {
                    indexOfCommand = i;
                    break;
                }
            }

            int templateLines = 1; // start at line 1

            // accumulate the number of lines the commands take up in the template source
            for (int i = 0; i <= indexOfCommand; i++)
            {
                TemplateCommand cmd = commands[i];
                templateLines += cmd.TemplateLines;
            }

            return templateLines += line - executableLines;
        }

        public override string ToString()
        {
            return "[" + BackingFile + "]";
        }
    }
}