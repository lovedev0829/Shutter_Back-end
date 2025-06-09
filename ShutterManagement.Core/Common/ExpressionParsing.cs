using ShutterManagement.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
        public enum TokenType
        {
            Number,
            Identifier,
            Plus,
            Minus,
            Multiply,
            Divide,
            GreaterThan,
            LessThan,
            Equals,
            NotEquals,
            LeftParen,
            RightParen,
            Comma,
            EndOfInput,
            Unknown
        }

        public class Token
        {
            public TokenType Type { get; }
            public string Text { get; }

            public Token(TokenType type, string text)
            {
                Type = type;
                Text = text;
            }

            public override string ToString() => $"{Type}: {Text}";
        }

        public class Lexer
        {
            private readonly string _input;
            private int _pos;

            public Lexer(string input)
            {
                _input = input;
            }

            public Token NextToken()
            {
                while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos])) _pos++;

                if (_pos >= _input.Length) return new Token(TokenType.EndOfInput, "");

                char ch = _input[_pos];

                if (char.IsDigit(ch))
                {
                    int start = _pos;
                    while (_pos < _input.Length && char.IsDigit(_input[_pos])) _pos++;
                    return new Token(TokenType.Number, _input.Substring(start, _pos - start));
                }

                if (char.IsLetter(ch))
                {
                    int start = _pos;
                    while (_pos < _input.Length && char.IsLetterOrDigit(_input[_pos])) _pos++;
                    string id = _input.Substring(start, _pos - start);
                    return new Token(TokenType.Identifier, id);
                }

                _pos++;
                return ch switch
                {
                    '+' => new Token(TokenType.Plus, "+"),
                    '-' => new Token(TokenType.Minus, "-"),
                    '*' => new Token(TokenType.Multiply, "*"),
                    '/' => new Token(TokenType.Divide, "/"),
                    '(' => new Token(TokenType.LeftParen, "("),
                    ')' => new Token(TokenType.RightParen, ")"),
                    ',' => new Token(TokenType.Comma, ","),
                    '>' => new Token(TokenType.GreaterThan, ">"),
                    '<' => new Token(TokenType.LessThan, "<"),
                    '=' when PeekChar() == '=' => AdvanceAndReturn(TokenType.Equals, "=="),
                    '!' when PeekChar() == '=' => AdvanceAndReturn(TokenType.NotEquals, "!="),
                    _ => new Token(TokenType.Unknown, ch.ToString())
                };
            }

            private char PeekChar() => _pos < _input.Length ? _input[_pos] : '\0';

            private Token AdvanceAndReturn(TokenType type, string text)
            {
                _pos++;
                return new Token(type, text);
            }
        }

        public class ApiParseResult
        {
            public bool IsValid => Errors.Count == 0;
            public List<string> Errors { get; } = new();
        }

        public class Parser
        {
            private readonly Lexer _lexer;
            private Token _current;
            private readonly ApiParseResult _result = new();

            public Parser(string input)
            {
                _lexer = new Lexer(input);
                _current = _lexer.NextToken();
            }

            private void NextToken() => _current = _lexer.NextToken();

            public ApiParseResult ParseExpression()
            {
                ParseTernary();

                if (_current.Type != TokenType.EndOfInput)
                    AddError($"Unexpected token at end: {_current.Text}");

                return _result;
            }

            private void AddError(string message)
            {
                _result.Errors.Add(message);
            }

            private void ParseTernary()
            {
                if (_current.Type == TokenType.Identifier && _current.Text == "if")
                {
                    NextToken();
                    if (!Expect(TokenType.LeftParen)) return;
                    ParseComparison();
                    if (!Expect(TokenType.Comma)) return;
                    ParseExpression();
                    if (!Expect(TokenType.Comma)) return;
                    ParseExpression();
                    Expect(TokenType.RightParen);
                }
                else
                {
                    ParseComparison();
                }
            }

            private void ParseComparison()
            {
                ParseTerm();
                if (_current.Type is TokenType.GreaterThan or TokenType.LessThan or TokenType.Equals or TokenType.NotEquals)
                {
                    NextToken();
                    ParseTerm();
                }
            }

            private void ParseTerm()
            {
                ParseFactor();
                while (_current.Type is TokenType.Multiply or TokenType.Divide)
                {
                    NextToken();
                    ParseFactor();
                }
            }

            private void ParseFactor()
            {
                if (_current.Type is TokenType.Number or TokenType.Identifier)
                {
                    NextToken();
                }
                else if (_current.Type == TokenType.LeftParen)
                {
                    NextToken();
                    ParseExpression();
                    Expect(TokenType.RightParen);
                }
                else
                {
                    AddError($"Unexpected token: {_current.Text}");
                    NextToken(); // Skip the invalid token
                }
            }

            private bool Expect(TokenType type)
            {
                if (_current.Type != type)
                {
                    AddError($"Expected {type} but found '{_current.Text}'");
                    return false;
                }
                NextToken();
                return true;
            }
        }

       


}
