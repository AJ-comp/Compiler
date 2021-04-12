using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR
{
    public class IRUserDefTypeList : Dictionary<string, IRStructDefInfo>
    {
        public static IRUserDefTypeList Instance
        {
            get
            {
                if (_instance == null) _instance = new IRUserDefTypeList();

                return _instance;
            }
        }

        private static IRUserDefTypeList _instance;
        private IRUserDefTypeList() { }
    }
}
