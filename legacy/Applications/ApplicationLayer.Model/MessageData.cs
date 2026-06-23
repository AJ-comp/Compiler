namespace ApplicationLayer.Models
{
    public enum MessageKind { Warning, Error, Information, Question }

    public class MessageData
    {
        public MessageKind Kind { get; }
        public string Message { get; }

        public MessageData(MessageKind kind, string message)
        {
            Kind = kind;
            Message = message;
        }
    }
}
