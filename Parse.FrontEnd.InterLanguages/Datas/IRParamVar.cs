using Parse.FrontEnd.InterLanguages.Datas.Types;

namespace Parse.MiddleEnd.IR.Datas
{
    public enum VarPassWay { CallByValue, CallByAddress };

    public class IRParamVar : IRVar
    {
        public IRParamVar(VarPassWay passWay, string name, int block, int offset, int length, string comment="")
        {
            PassWay = passWay;
            Comment = comment;
            _name = name;
            _block = block;
            _offset = offset;
            _length = length;
        }

        public VarPassWay PassWay { get; }
        public string Comment { get; }

        public string Name => _name;
        public int Block => _block;
        public int Offset => _offset;
        public int Length => _length;

        private string _name;
        private int _block;
        private int _offset;
        private int _length;
    }
}
