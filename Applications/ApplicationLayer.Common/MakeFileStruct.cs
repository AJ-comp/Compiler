using System;
using System.Collections.Generic;

namespace ApplicationLayer.Common
{
    public class MakeFileStruct
    {
        public string Result { get; }
        public List<string> Materials { get; } = new List<string>();
        public List<string> Recipe { get; } = new List<string>();

        public string StartingMessage { get; set; } = string.Empty;
        public string EndingMessage { get; set; } = string.Empty;


        public MakeFileStruct(string result, IEnumerable<string> materials, IEnumerable<string> recipe)
        {
            Result = result;
            Materials.AddRange(materials);
            Recipe.AddRange(recipe);
        }

        public MakeFileStruct(string result, IEnumerable<string> materials, params string[] recipe)
        {
            Result = result;
            Materials.AddRange(materials);
            Recipe.AddRange(recipe);
        }

        public MakeFileStruct(string result, IEnumerable<string> recipe)
        {
            Result = result;
            Recipe.AddRange(recipe);
        }

        public MakeFileStruct(string result, params string[] recipe)
        {
            Result = result;
            Recipe.AddRange(recipe);
        }

        public override string ToString()
        {
            var header = Result + " : ";

            foreach (var material in Materials) header += " " + material;
            header += Environment.NewLine;

            var content = (StartingMessage.Length > 0) 
                                ? "\t" + "@echo" + StartingMessage + Environment.NewLine
                                : string.Empty;

            foreach (var recipe in Recipe) content += "\t" + recipe + Environment.NewLine;

            content += (EndingMessage.Length > 0)
                        ? "\t" + EndingMessage + Environment.NewLine
                        : string.Empty;

            return header + content;
        }
    }
}
