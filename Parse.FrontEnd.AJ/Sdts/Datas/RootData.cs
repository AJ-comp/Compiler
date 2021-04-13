using Parse.FrontEnd.AJ.Sdts.AstNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public class RootData
    {
        public HashSet<ProgramNode> ProgramNodes { get; } = new HashSet<ProgramNode>();
    }
}
