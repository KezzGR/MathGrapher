using System.Globalization;

namespace MathGrapher.Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test("-sin(x)^2 - cos(x)^2", Math.PI/6, 2);
        }

        static void Test(string expr, double x, double expected)
        {
            try
            {
                Console.WriteLine($"Тест: {expr}");
                string debug = ExpressionParser.ToDebugString(expr, x);
                Console.WriteLine($"  ОПН: {debug}");
                double result = ExpressionParser.Evaluate(expr, x);
                if (Math.Abs(result - expected) < 1e-9)
                    Console.WriteLine($"  [OK] результат = {result}");
                else
                    Console.WriteLine($"  [FAIL] ожидалось {expected}, получено {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ERR] {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    public static class ExpressionParser
    {
        private static readonly Dictionary<char, int> Precedence = new Dictionary<char, int>
        {
            { '+', 1 }, { '-', 1 }, { '*', 2 }, { '/', 2 }, { '^', 3 }
        };

        private static readonly Dictionary<string, Func<double, double>> Functions =
            new Dictionary<string, Func<double, double>>(StringComparer.OrdinalIgnoreCase)
        {
            { "sin", Math.Sin }, { "cos", Math.Cos }, { "sqrt", Math.Sqrt },
            { "abs", Math.Abs }, { "log", Math.Log }, { "exp", Math.Exp }
        };

        private static readonly Dictionary<string, double> Constants =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            { "pi", Math.PI }, { "e", Math.E }
        };

        public static string ToDebugString(string expression, double x)
        {
            var queue = ShuntingYard(expression, x);
            var items = new List<string>();
            foreach (var token in queue)
            {
                items.Add(token.Type switch
                {
                    TokenType.Number => ((double)token.Value).ToString(CultureInfo.InvariantCulture),
                    TokenType.Operator => ((char)token.Value).ToString(),
                    TokenType.Function => (string)token.Value,
                    TokenType.LeftParen => "(",
                    TokenType.RightParen => ")",
                    _ => "?"
                });
            }
            return string.Join(" ", items);
        }

        public static double Evaluate(string expression, double x)
        {
            Queue<Token> output = ShuntingYard(expression, x);
            return EvaluateRPN(output);
        }

        private static Queue<Token> ShuntingYard(string expression, double x)
        {
            var output = new Queue<Token>();
            var operators = new Stack<Token>();

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (char.IsWhiteSpace(c)) continue;

                if (char.IsDigit(c) || c == '.')
                {
                    string num = "";
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                        num += expression[i++];
                    i--;
                    output.Enqueue(Token.Number(double.Parse(num, CultureInfo.InvariantCulture)));
                }
                else if (c == 'x')
                {
                    output.Enqueue(Token.Number(x));
                }
                else if (char.IsLetter(c))
                {
                    string name = "";
                    while (i < expression.Length && char.IsLetter(expression[i]))
                        name += expression[i++];
                    i--;
                    if (Constants.TryGetValue(name, out double cnst))
                        output.Enqueue(Token.Number(cnst));
                    else if (Functions.ContainsKey(name))
                        operators.Push(Token.Function(name));
                    else throw new Exception($"Неизвестное имя: {name}");
                }
                else if (c == '(')
                {
                    operators.Push(Token.LeftParen());
                }
                else if (c == ')')
                {
                    while (operators.Count > 0 && operators.Peek().Type != TokenType.LeftParen)
                        output.Enqueue(operators.Pop());
                    if (operators.Count == 0) throw new Exception("Несогласованные скобки");
                    operators.Pop(); // '('
                    if (operators.Count > 0 && operators.Peek().Type == TokenType.Function)
                        output.Enqueue(operators.Pop());
                }
                else if (Precedence.ContainsKey(c))
                {
                    if (c == '-' && (i == 0 || expression[i - 1] == '(' || Precedence.ContainsKey(expression[i - 1])))
                    {
                        output.Enqueue(Token.Number(-1.0));
                        operators.Push(Token.Operator('*'));
                    }
                    else
                    {
                        while (operators.Count > 0 && operators.Peek().Type == TokenType.Operator)
                        {
                            char stackOp = (char)operators.Peek().Value;
                            if (Precedence.TryGetValue(stackOp, out int stackPrec))
                            {
                                var currPrec = Precedence[c];

                                if ((c != '^' && stackPrec >= currPrec) || (c == '^' && stackPrec > currPrec))
                                {
                                    output.Enqueue(operators.Pop());
                                }
                                else break;
                            }
                            else break;

                        }
                        operators.Push(Token.Operator(c));
                    }
                }
                else
                {
                    throw new Exception($"Недопустимый символ: '{c}'");
                }
            }

            while (operators.Count > 0)
            {
                Token token = operators.Pop();

                if (token.Type == TokenType.LeftParen) throw new Exception("Несогласованные скобки");

                output.Enqueue(token);
            }

            return output;
        }

        private static double EvaluateRPN(Queue<Token> rpnQueue)
        {
            var stack = new Stack<double>();

            while (rpnQueue.Count > 0)
            {
                Token token = rpnQueue.Dequeue();

                switch (token.Type)
                {
                    case TokenType.Number:
                        stack.Push((double)token.Value);
                        break;

                    case TokenType.Operator:
                        char op = (char)token.Value;
                        double right = stack.Pop();
                        double left = stack.Pop();
                        double result = op switch
                        {
                            '+' => left + right,
                            '-' => left - right,
                            '*' => left * right,
                            '/' => left / right,
                            '^' => Math.Pow(left, right),
                            _ => throw new Exception($"Неизвестный оператор: {op}")
                        };

                        stack.Push(result);
                        break;

                    case TokenType.Function:
                        string funcName = (string)token.Value;
                        double arg = stack.Pop();
                        var func = Functions[funcName];
                        stack.Push(func(arg));
                        break;

                    default:
                        throw new Exception($"Неожиданный токен: {token.Type}");
                }
            }

            if (stack.Count != 1) throw new Exception($"Ошибка вычисления: неверное число операндов");

            return stack.Pop();
        }
    }

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