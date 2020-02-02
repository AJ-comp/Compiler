using ApplicationLayer.Models.SolutionPackage;
using System.Collections.Generic;

namespace ApplicationLayer.Models
{
    public class ChangedFileList : List<HirStruct>
    {

        private static ChangedFileList own;
        public static ChangedFileList Instance
        {
            get
            {
                if (ChangedFileList.own == null) ChangedFileList.own = new ChangedFileList();

                return ChangedFileList.own;
            }
        }
    }
}
