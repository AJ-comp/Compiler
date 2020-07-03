namespace Parse.FrontEnd.InterLanguages.Datas.Types
{
    public class Short : Integer
    {
        public Short()
        {
        }

        public short RealValue { get; }

        public override int Size => 16;

        public override string ToString() => "i16";
    }
}
