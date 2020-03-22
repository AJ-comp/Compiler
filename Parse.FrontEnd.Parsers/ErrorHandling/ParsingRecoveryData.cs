namespace Parse.FrontEnd.Parsers.ErrorHandling
{
    public class ParsingRecoveryData
    {
        public TokenData RecoveryToken { get; }
        public string RecoveryMessage { get; }

        public ParsingRecoveryData(TokenData recoveryToken, string reconveryMessage)
        {
            RecoveryToken = recoveryToken;
            RecoveryMessage = reconveryMessage;
        }

        public override string ToString() => RecoveryMessage;
    }
}
