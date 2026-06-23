using Compile.AJ;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace CommandPrompt.Lint
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand();
            AJCompiler compiler = new AJCompiler();

            cmd.Name = "ajlint";
            //            cmd.Description = Resource.AJBuild;
            //            cmd.Add(new New());
            //            cmd.Add(new Sln());
            //            cmd.Add(new Build());
            //            cmd.Add(new Arch());

            while (true)
            {
                var command = Console.ReadLine();




                await cmd.InvokeAsync(command);
            }
        }
    }
}
