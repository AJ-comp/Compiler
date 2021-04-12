using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public class RootData
    {
        public HashSet<ProgramNode> ProgramNodes { get; } = new HashSet<ProgramNode>();
    }
}
