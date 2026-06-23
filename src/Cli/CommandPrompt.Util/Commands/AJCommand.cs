using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace CommandPrompt.Util.Commands
{
    public abstract class AJCommand : Command
    {
        protected AJCommand(string name, string description = null) : base(name, description)
        {
        }

        public int Execute(Func<int> action)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return ex.HResult;
            }
        }
    }
}
