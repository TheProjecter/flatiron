using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Flatiron.Parsing
{
    // this shit is so janky.
    class CommandReader : IDisposable
    {
        StreamReader reader;
        bool directive;

        public CommandReader(Stream template)
        {
            reader = new StreamReader(template);
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public TemplateCommand ReadCommand()
        {
            if (reader.EndOfStream) return null;

            var cmd = new TemplateCommand();
            string command = ReadUntilToken(directive ? "%>" : "<%");
            cmd.TemplateLines = command.GetNumLines();

            if (directive)
                if (EatTokenFromStart("#", ref command) || EatTokenFromStart("include", ref command))
                    cmd.Type = TemplateCommandType.Include;
                else if (EatTokenFromStart("=", ref command))
                    cmd.Type = TemplateCommandType.Expression;
                else if (EatTokenFromStart(":", ref command) || EatTokenFromStart("set-section", ref command))
                    cmd.Type = TemplateCommandType.OutputToSection;
                else if (EatTokenFromStart("!", ref command) || EatTokenFromStart("get-section", ref command))
                    cmd.Type = TemplateCommandType.WriteChildSection;
                else if (EatTokenFromStart("@", ref command) || EatTokenFromStart("set-parent", ref command))
                    cmd.Type = TemplateCommandType.SetParentTemplate;
                else
                    cmd.Type = TemplateCommandType.Code;
            else
                cmd.Type = TemplateCommandType.Literal;

            cmd.Body = command;

            // the main logic is read literal, read directive, rinse, lather, repeat.
            directive = !directive;

            // don't return empty literals
            return (cmd.Type == TemplateCommandType.Literal && cmd.Body == "") ? ReadCommand() : cmd;
        }


        private string ReadUntilToken(string token)
        {
            Queue<char> q = new Queue<char>(token.Length);
            char[] b = new char[1];
            StringBuilder sb = new StringBuilder();
            bool readZero = false;

            while (true)
            {
                if (q.Count < token.Length && !readZero)
                {
                    if (reader.Read(b, 0, 1) != 0)
                        q.Enqueue(b[0]);
                    else readZero = true;
                }
                else
                {
                    int i = 0;
                    if (q.All(delegate(char c) { return token[i++] == c; }) || (readZero && q.Count == 0))
                        return sb.ToString();
                    else
                        sb.Append(q.Dequeue());
                }
            }
        }

        private bool EatTokenFromStart(string token, ref string str)
        {
            if (str.StartsWith(token))
            {
                str = str.Substring(token.Length);
                return true;
            }
            else return false;
        }
    }
}
