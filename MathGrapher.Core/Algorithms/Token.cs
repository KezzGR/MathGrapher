using System;

namespace MathGrapher.Core.Algorithms
{
    public enum TokenType
    {
        Number,
        Operator,
        Function,
        LeftParen,
        RightParen
    }

    public class Token
    {
        public TokenType Type { get; }
        public object Value { get; }

        private Token(TokenType type, object value = null)
        {
            Type = type;
            Value = value;
        }

        public static Token Number(double d) => new Token(TokenType.Number, d);
        public static Token Operator(char op) => new Token(TokenType.Operator, op);
        public static Token Function(string name) => new Token(TokenType.Function, name);
        public static Token LeftParen() => new Token(TokenType.LeftParen);
        public static Token RightParen() => new Token(TokenType.RightParen);
    }
}