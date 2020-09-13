using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC
{
    public class MiniCDefineTable : Dictionary<string, string>
    {
        public static MiniCDefineTable Instance
        {
            get
            {
                if (_instance == null) _instance = new MiniCDefineTable();

                return _instance;
            }
        }

        private static MiniCDefineTable _instance;
    }
}
