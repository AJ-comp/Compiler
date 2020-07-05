namespace Parse.MiddleEnd.IR.Datas.Types
{
    public class Bit : Integer
    {
        public Bit()
        {
        }

        public override int Size => 1;

        public override string ToString() => "i1";
    }
}
