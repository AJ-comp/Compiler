namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public class MiniCSdtsParams : SdtsParams
    {
        public MiniCSymbolTable SymbolTable { get; private set; } = new MiniCSymbolTable();

        public MiniCSdtsParams(int blockLevel, int offset) : base(blockLevel, offset)
        {
        }

        public override SdtsParams Clone()
        {
            var result = new MiniCSdtsParams(BlockLevel, Offset)
            {
                SymbolTable = SymbolTable
            };

            return result;
        }

        public override SdtsParams CloneForNewBlock()
        {
            return new MiniCSdtsParams(BlockLevel, Offset);
        }
    }
}
