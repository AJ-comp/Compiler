using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class MiniCFileTreeNodeModel : FileTreeNodeModel
    {
        public string Data { get; set; }

        public MiniCFileTreeNodeModel(string path, string fileName) : base(path, fileName)
        {
        }
    }
}
