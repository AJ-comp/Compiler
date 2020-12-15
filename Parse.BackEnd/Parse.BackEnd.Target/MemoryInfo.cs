using System.Diagnostics;

namespace Parse.BackEnd.Target
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class MemoryInfo
    {
        public uint StartAddr { get; }
        public uint Size { get; }
        public uint EndAddr => StartAddr + Size;

        public MemoryInfo(uint startAddr, uint size)
        {
            StartAddr = startAddr;
            Size = size;
        }

        private string DebuggerDisplay
            => string.Format("Start address: {0}, End address: {1}",
                                        StartAddr, EndAddr);
    }
}
