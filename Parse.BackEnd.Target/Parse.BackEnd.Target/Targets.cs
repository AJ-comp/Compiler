namespace Parse.BackEnd.Target
{
    public interface ITarget
    {
        string Name { get; }

        string Explain { get; }
    }

    public class ARM : ITarget
    {
        public string Name { get; protected set; }
        public string Explain { get; protected set; }

        public ARM()
        {
            this.Name = "ARM";
        }
    }

    public class AVR : ITarget
    {
        public string Name { get; protected set; }
        public string Explain { get; protected set; }

        public AVR()
        {
            this.Name = "AVR";
        }
    }
}
