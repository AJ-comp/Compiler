using CommandPrompt.Builder.Properties;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;

namespace CommandPrompt.Builder.Commands
{
    public class Arch : AJBuildCommand
    {
        public Arch() : base(nameof(Arch).ToLower(), Resource.Arch)
        {
            Add(new Option<string>("--list", Resource.ListArch));

            Handler = CommandHandler.Create<string>(HandleListArch);
        }


        private int HandleListArch(string list)
        {
            Console.WriteLine($"arm : {Resource.ArmArch}");
            Console.WriteLine($"avr : {Resource.AvrArch}");

            return 0;
        }
    }
}
