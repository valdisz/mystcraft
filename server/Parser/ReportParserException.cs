namespace advisor
{
    [System.Serializable]
    public class ReportParserException : System.Exception
    {
        public ReportParserException() { }
        public ReportParserException(string message) : base(message) { }
        public ReportParserException(string message, System.Exception inner) : base(message, inner) { }
        protected ReportParserException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
