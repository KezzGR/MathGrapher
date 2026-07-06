using System;
using System.Collections.Generic;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MathGrapher.Core.Algorithms
{
    public static class ExpressionParser
    {
        private static readonly Dictionary<char, int> _precedance = new Dictionary<char, int>
        {
            { '+', 1 },
            { '-', 1 },
            { '*', 2 },
            { '/', 2 },
            { '^', 3}
        };

        private static readonly Dictionary<string, Func<double, double>> _functions = new Dictionary<string, Func<double, double>>(StringComparer.OrdinalIgnoreCase)
        {
            { "sin", Math.Sin },
            { "cos", Math.Cos },
            { "sqrt", Math.Sqrt },
            { "abs", Math.Abs },
            { "log", Math.Log },
            { "exp", Math.Exp }
        };

        private static readonly Dictionary<string, double> _constants = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            { "pi", Math.PI },
            { "e", Math.E },
        };

        private static Queue<object> ShuntingYard(string expression, double x)
        {
            var output = new Queue<object>();
            var operators = new Stack<object>();

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsWhiteSpace(c)) continue;

                if (char.IsDigit(c) || c == '.')
                {
                    string number = "";

                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    {
                        number += expression[i];
                        i++;
                    }
                    i--;

                    output.Enqueue(double.Parse(number, CultureInfo.InvariantCulture));
                }
                else if (c == 'x')
                {
                    output.Enqueue(x);
                }
                else if (char.IsLetter(c))
                {
                    string name = "";

                    while (i < expression.Length && char.IsLetter(c))
                    {
                        name += expression[i];
                        i++;
                    }
                    i--;

                    if (_constants.TryGetValue(name, out double constantValue))
                    {
                        output.Enqueue(constantValue);
                    }
                    else if (_functions.ContainsKey(name))
                    {
                        operators.Push(name);
                    }
                    else
                    {
                        throw new Exception($"Неизвестное имя: {name}");
                    }
                }
                else if (c == '(')
                {
                    operators.Push(c);
                }
                else if (c == ')')
                {
                    while (operators.Count > 0 && !(operators.Peek() is char ch && ch == '('))
                    {
                        output.Enqueue(operators.Pop());
                    }

                    if (operators.Count == 0) throw new Exception("Несогласованные скобки");

                    operators.Pop();

                    if (operators.Count > 0 && operators.Peek() is string func)
                    {
                        output.Enqueue(operators.Pop());
                    }
                }
                else if (_precedance.ContainsKey(c))
                {
                    if (c == '-' && (i == 0 || expression[i - 1] == '(' || _precedance.ContainsKey(expression[i - 1])))
                    {
                        output.Enqueue(-1.0);
                        operators.Push('*');
                    }
                    else
                    {
                        while (operators.Count > 0 && operators.Peek() is char op && _precedance.ContainsKey(op))
                        {
                            var currentPrec = _precedance[c];
                            var stackPrec = _precedance[op];

                            if ((c != '^' && stackPrec >= currentPrec) || (c == '^' && stackPrec > currentPrec))
                            {
                                output.Enqueue(operators.Pop());
                            }
                            else
                                break;

                            operators.Push(c);
                        }
                    }
                }
                else
                {
                    throw new Exception($"Недопустимый символ: '{c}'");
                }
            }

            while (operators.Count > 0)
            {
                var op = operators.Pop();

                if (op is char ch && ch == '(') throw new Exception("Несогласованные скобки");

                output.Enqueue(op);
            }

            return output;
        }

        private static double EvaluateRPN(Queue<object> rpnQueue, double x)
        {
            var stack = new Stack<double>();

            while (rpnQueue.Count > 0)
            {
                var token = rpnQueue.Dequeue();

                if (token is double number)
                {
                    stack.Push(number);
                }
                else if (token is char op)
                {
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
                }
                else if (token is string funcName)
                {
                    var arg = stack.Pop();
                    var func = _functions[funcName];
                    stack.Push(func(arg));
                }
                else
                {
                    throw new Exception($"Неожиданный токен: {token}");
                }
            }

            if (stack.Count != 1) throw new Exception($"Ошибка вычисления: неверное число операндов");

            return stack.Pop();
        }
    }
}