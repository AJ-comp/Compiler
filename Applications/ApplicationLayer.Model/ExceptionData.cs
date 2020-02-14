namespace ApplicationLayer.Models
{
    public enum ExceptionKind { Warning, Error }

    public class ExceptionData
    {
        public ExceptionKind Kind { get; }
        public string Message { get; }

        public ExceptionData(ExceptionKind kind, string message)
        {
            Kind = kind;
            Message = message;
        }
    }
}
