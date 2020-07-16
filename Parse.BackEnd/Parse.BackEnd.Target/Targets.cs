namespace Parse.BackEnd.Target
{
    public class Target
    {
        public string Name { get; protected set; }
        public string Explain { get; protected set; }
    }

    public class AVR : Target
    {
        public AVR()
        {
            this.Name = "AVR";
        }
    }
}
