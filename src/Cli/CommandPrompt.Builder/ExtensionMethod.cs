using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace CommandPrompt.Builder
{
    public static class ExtensionMethod
    {
        public static Command WithHandler(this Command command, string name)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(Command).GetMethod(name, flags);

            var handler = CommandHandler.Create(method!);
            command.Handler = handler;
            return command;
        }
    }
}
