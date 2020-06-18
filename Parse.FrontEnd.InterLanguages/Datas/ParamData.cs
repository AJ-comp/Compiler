namespace Parse.FrontEnd.InterLanguages.Datas
{
    public enum VarPassWay { CallByValue, CallByAddress };

    public class ParamData
    {
        public int BIndex { get; }
        public int OIndex { get; }
        public string Name { get; }

        public VarPassWay PassWay { get; }
        public string Comment { get; }

        public ParamData(int bIndex, int oIndex, string name, VarPassWay passWay, string comment)
        {
            BIndex = bIndex;
            OIndex = oIndex;
            Name = name;
            PassWay = passWay;
            Comment = comment;
        }

    }
}
