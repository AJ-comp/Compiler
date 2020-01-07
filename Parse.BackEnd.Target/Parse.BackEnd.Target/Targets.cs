namespace Parse.BackEnd.Target
{
    public class Target
    {
        public string Name { get; protected set; }
        public string Explain { get; protected set; }
    }

    public class ARM : Target
    {
        public ARM()
        {
            this.Name = "ARM";
        }
    }

    public class AVR : Target
    {
        public AVR()
        {
            this.Name = "AVR";
        }
    }
}
