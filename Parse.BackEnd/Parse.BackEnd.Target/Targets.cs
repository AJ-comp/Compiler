namespace Parse.BackEnd.Target
{
    public abstract class Target
    {
        public string Name { get; protected set; }
        public string Explain { get; protected set; }


        public abstract string LLVM_IR_MCPU_String();
        public abstract string GCC_IR_MCPU_String();
    }

    public class AVR : Target
    {
        public AVR()
        {
            this.Name = "AVR";
        }

        public override string GCC_IR_MCPU_String() => "atmega";
        public override string LLVM_IR_MCPU_String() => "atmega";
    }
}
