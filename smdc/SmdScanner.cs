using System;
using System.Globalization;
using System.IO;

namespace smdc
{
    public class SmdScanner
    {
        private readonly TextReader _reader;
        private int _line, _column;
        private char _ch;
        private bool _eof;

        public SmdScanner(TextReader reader)
        {
            _ch = ' ';
            _reader = reader;
        }

        public Token Next()
        {
            while (char.IsWhiteSpace(_ch))
                NextChar();

            if (_ch == '#')
            {
                int startLine = _line;

                while (_line == startLine && !_eof)
                    NextChar();

                return Next();
            }

            Token tk = new Token();

            tk.Line = _line;
            tk.Column = _column;

            if (_eof)
            {
                tk.Type = TokenType.Eof;
            }
            else if (_ch == '"')
            {
                MatchString(tk);
            }
            else if (char.IsNumber(_ch))
            {
                MatchNumber(tk);
            }
            else
            {
                MatchKeyword(tk);
            }

            return tk;
        }

        private void MatchString(Token tk)
        {
            string str = "";

            NextChar();
            while (!_eof && _ch != '"')
            {
                str += _ch;
                NextChar();
            }

            if (!_eof)
                NextChar();

            tk.Type = TokenType.String;
            tk.Text = str;
        }

        private void MatchNumber(Token tk)
        {
            NumberStyles style = NumberStyles.Integer;
            string str = "";
            uint value;

            str += _ch;
            NextChar();

            if (str == "0" && (_ch == 'x' || _ch == 'X'))
            {
                str = "0";
                style = NumberStyles.HexNumber;
                NextChar();
            }

            while (!_eof)
            {
                if (!char.IsNumber(_ch))
                {
                    if (style != NumberStyles.HexNumber)
                        break;

                    if (!(_ch >= 'a' && _ch <= 'f') && !(_ch >= 'A' && _ch <= 'F'))
                        break;
                }

                str += _ch;
                NextChar();
            }

            value = uint.Parse(str, style);

            tk.Type = TokenType.Number;
            tk.Text = str;
            tk.Value = value;
        }

        private void MatchKeyword(Token tk)
        {
            string str = "";

            while (!_eof && (_ch == '_' || char.IsLetter(_ch)))
            {
                str += _ch;
                NextChar();
            }

            tk.Text = str;

            if (str.Equals("device", StringComparison.InvariantCultureIgnoreCase))
                tk.Type = TokenType.Device;
            else if (str.Equals("version", StringComparison.InvariantCultureIgnoreCase))
                tk.Type = TokenType.Version;
            else if (str.Equals("load", StringComparison.InvariantCultureIgnoreCase))
                tk.Type = TokenType.Load;
        }

        private void NextChar()
        {
            int chR = _reader.Read();

            if (chR == -1)
            {
                _eof = true;
                return;
            }

            char ch = (char)chR;

            if (ch == '\n')
            {
                _line++;
                _column = 0;
                NextChar();
            }
            else if (ch == '\r')
            {
                NextChar();
            }
            else
            {
                _ch = ch;
                _column++;
            }
        }
    }
}
