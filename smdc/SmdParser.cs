using System;

namespace smdc
{
    public class SmdParser
    {
        private readonly SmdScanner _scanner;
        private Token _tk;

        public SmdParser(SmdScanner scanner)
        {
            _scanner = scanner;
        }

        public SmdScript Parse()
        {
            SmdScript script = new SmdScript();
            Token tk;

            Next();
            while (_tk.Type != TokenType.Eof)
            {
                Match(_tk.Type, out tk);

                if (tk.Type == TokenType.Device)
                {
                    Match(TokenType.String, out tk);
                    script.Device = tk.Text;
                }
                else if (tk.Type == TokenType.Version)
                {
                    Match(TokenType.String, out tk);
                    script.Version = tk.Text;
                }
                else if (tk.Type == TokenType.Load)
                {
                    LoadEntry entry = new LoadEntry();

                    Match(TokenType.String, out tk);
                    entry.Source = tk.Text;

                    Match(TokenType.String, out tk);
                    entry.Name = tk.Text;

                    if (TryMatch(TokenType.Number, out tk))
                        entry.Address = tk.Value;

                    if (TryMatch(TokenType.Number, out tk))
                        entry.Size = tk.Value;

                    if (TryMatch(TokenType.Number, out tk))
                        entry.FileSystem = tk.Value;

                    script.Entries.Add(entry);
                }
                else
                {
                    throw new Exception(string.Format("Malformed SMD script at {0}:{1}: unexpected {2}", tk.Line,
                        tk.Column, tk.Type));
                }
            }

            return script;
        }

        private void Match(TokenType type, out Token tk)
        {
            if (_tk.Type != type)
                throw new Exception(string.Format("Malformed SMD script at {0}:{1}: expected {2}, got {3}", _tk.Line,
                    _tk.Column, type, _tk.Type));

            tk = _tk;
            Next();
        }

        private bool TryMatch(TokenType type, out Token tk)
        {
            tk = null;

            if (_tk.Type != type)
                return false;

            tk = _tk;
            Next();
            return true;
        }

        private void Next()
        {
            _tk = _scanner.Next();
        }
    }
}
