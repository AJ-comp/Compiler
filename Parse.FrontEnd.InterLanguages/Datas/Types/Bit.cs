namespace Parse.FrontEnd.InterLanguages.Datas.Types
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
