using Parse.BackEnd.Target.ARM;

namespace Parse.BackEnd.Target.ARMv7.MSeries
{
    public abstract class ARMv7_M : Arm
    {
        public ARMv7_M()
        {
        }

        public override string GCC_IR_MCPU_String() => "arm";
        public override string LLVM_IR_MCPU_String() => "arm";
    }

    public class ArmALFormat
    {
        public string OpCode { get; set; }
        public string Operand1 { get; set; }
        public string Operand2 { get; set; }
        public string Comment { get; set; }
    }
}
