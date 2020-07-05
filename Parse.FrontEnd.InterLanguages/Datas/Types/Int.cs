namespace Parse.MiddleEnd.IR.Datas.Types
{
    public class Int : Integer
    {
        public Int()
        {
        }

        public override int Size => 32;

        public override string ToString() => "i32";
    }
}
