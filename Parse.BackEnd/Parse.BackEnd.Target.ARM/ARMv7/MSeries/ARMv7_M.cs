using Parse.BackEnd.Target.ARM;

namespace Parse.BackEnd.Target.ARMv7.MSeries
{
    public class ARMv7_M : Arm
    {
        public ARMv7_M()
        {
            this.Name = "ARMv7_M";
        }
    }

    public class ArmALFormat
    {
        public string OpCode { get; set; }
        public string Operand1 { get; set; }
        public string Operand2 { get; set; }
        public string Comment { get; set; }
    }
}
