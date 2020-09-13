using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class DefinePrepData : IHasName
    {
        public string Name { get; private set; }
        public IEnumerable<TokenData> ReplaceString => _replaceString;

        public DefinePrepData(string name, IEnumerable<TokenData> replaceString)
        {
            Name = name;
            _replaceString.AddRange(replaceString);
        }

        private List<TokenData> _replaceString = new List<TokenData>();
    }
}
