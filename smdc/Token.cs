namespace smdc
{
    public class Token
    {
        public TokenType Type { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string Text { get; set; }
        public uint Value { get; set; }
    }
}
