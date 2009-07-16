
namespace Flatiron.Parsing
{
    enum TemplateCommandType
    {
        Literal,
        Code,
        Expression,
        Include,
        OutputToSection,
        WriteChildSection,
        SetParentTemplate
    }

    class TemplateCommand
    {
        public TemplateCommandType Type;
        public string Body;
        public int TemplateLines;
        public int ExecutableLines;
    }
}
