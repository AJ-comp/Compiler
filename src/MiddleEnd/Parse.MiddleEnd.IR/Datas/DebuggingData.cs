using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Datas
{
    public class DebuggingData
    {
        public DebuggingData(int startLine, int endLine, int startColumn, int endColumn, string source)
        {
            bMeanIndex = true;
            StartLine = startLine;
            EndLine = endLine;
            StartColumn = startColumn;
            EndColumn = endColumn;
            Source = source;
        }

        public DebuggingData(string source)
        {
            bMeanIndex = false;
            Source = source;
        }


        public static DebuggingData CreateDummy(string source = "") => new DebuggingData(source);


        public bool bMeanIndex { get; } = false;

        public int StartLine { get; }
        public int EndLine { get; }

        public int StartColumn { get; }
        public int EndColumn { get; }

        public string Source { get; }
    }
}
