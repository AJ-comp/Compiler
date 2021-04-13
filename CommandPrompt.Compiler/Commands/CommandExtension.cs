using System;
using System.CommandLine;

namespace CommandPrompt.Compiler.Commands
{
    public static class CommandExtension
    {
        public static int Execute(this Command obj, Func<int> action)
        {
            try
            {
                return action.Invoke();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

                return ex.HResult;
            }
        }
    }
}
