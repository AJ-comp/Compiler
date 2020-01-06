namespace Parse.BackEnd.Target.ARMv7.MSeries.CortexM3Models
{
    public class CortexM3 : ARMv7_M
    {
        public string DataSheet { get; protected set; }
        public MemoryInfo FlashMemory { get; set; }
        public MemoryInfo RAM { get; set; }
        public MemoryInfo EEPROM { get; set; }

        public CortexM3()
        {
            this.Name = "CortexM3";
        }
    }
}
