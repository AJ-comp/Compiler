using CommandPrompt.Util.Commands;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace CommandPrompt.Util
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand();

            cmd.Name = "ajutil";
//            cmd.Description = Resource.AJBuild;

            cmd.Add(new Print());

            return await cmd.InvokeAsync(args);
        }
    }
}
