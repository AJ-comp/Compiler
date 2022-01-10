using CommandPrompt.Builder.Commands;
using CommandPrompt.Builder.Properties;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace CommandPrompt.Builder
{
    class Program
    {
        // https://blog.elmah.io/how-to-create-a-colored-cli-with-system-commandline-and-spectre/
        static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand();

            cmd.Name = "ajbuild";
            cmd.Description = Resource.AJBuild;

            cmd.Add(new New());
            cmd.Add(new Sln());
            cmd.Add(new Build());
            cmd.Add(new Arch());

            return await cmd.InvokeAsync(args);
        }
    }
}
