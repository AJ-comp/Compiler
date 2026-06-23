using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class UndefinedTreeNodeModel : FileTreeNodeModel
    {
        public UndefinedTreeNodeModel(string path, string filename) : base(path, filename)
        {
        }
    }
}
