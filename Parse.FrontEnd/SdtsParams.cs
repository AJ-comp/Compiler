using System;

namespace Parse.FrontEnd
{
    public abstract class SdtsParams : ICloneable<SdtsParams>
    {
        public SdtsParams(int blockLevel, int offset)
        {
            BlockLevel = blockLevel;
            Offset = offset;
        }

        public int BlockLevel { get; set; }
        public int Offset { get; set; }

        public abstract SdtsParams Clone();
        public abstract SdtsParams CloneForNewBlock();
    }
}
