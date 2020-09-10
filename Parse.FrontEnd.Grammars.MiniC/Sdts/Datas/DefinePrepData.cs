namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class DefinePrepData : IHasName
    {
        public string Name { get; private set; }
        public string ReplaceString { get; }

        public DefinePrepData(string name, string replaceString)
        {
            Name = name;
            ReplaceString = replaceString;
        }

    }
}
