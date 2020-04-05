using ApplicationLayer.Models.SolutionPackage;
using System.Collections.Generic;

namespace ApplicationLayer.Models
{
    public class ChangedFileList : List<TreeNodeModel>
    {

        private static ChangedFileList instance;
        public static ChangedFileList Instance
        {
            get
            {
                if (ChangedFileList.instance == null) ChangedFileList.instance = new ChangedFileList();

                return ChangedFileList.instance;
            }
        }
    }
}
