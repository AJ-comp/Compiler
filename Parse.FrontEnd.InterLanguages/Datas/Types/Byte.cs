namespace Parse.FrontEnd.InterLanguages.Datas.Types
{
    public class Byte : Integer
    {
        public Byte()
        {
        }

        public byte RealValue { get; }

        public override int Size => 8;

        public override string ToString() => "i8";
    }
}
