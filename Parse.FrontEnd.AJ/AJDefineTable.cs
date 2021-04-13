using System.Collections.Generic;

namespace Parse.FrontEnd.AJ
{
    public class AJDefineTable : Dictionary<string, string>
    {
        public static AJDefineTable Instance
        {
            get
            {
                if (_instance == null) _instance = new AJDefineTable();

                return _instance;
            }
        }

        private static AJDefineTable _instance;
    }
}
