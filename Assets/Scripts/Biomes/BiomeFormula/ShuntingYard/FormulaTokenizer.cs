using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime;

namespace Polyhedral.FastFormula.ShuntingYard
{
    public static class FormulaTokenizer
    {
        public static List<FormulaToken> Tokenize(string expression)
        {
            List<FormulaToken> tokens = new();
            int i=0;

            while (i < expression.Length)
            {
                char c = expression[i];

                // Skip whitespaces
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // Numbers
                if (char.IsDigit(c) || c == '.')
                {
                    tokens.Add(new FormulaToken(TokenType.Number, TokenizeNumber(expression, ref i)));
                    continue;
                }

                // Operators
                if ("+-*/^><=".Contains(c))
                {
                    tokens.Add(new FormulaToken(TokenType.Operator, c.ToString()));
                    i++;
                    continue;
                }

                // Parentheses
                if (c == '(')
                {
                    tokens.Add(new FormulaToken(TokenType.LeftParen, "("));
                    i++;
                    continue;
                }

                if (c == ')')
                {
                    tokens.Add(new FormulaToken(TokenType.RightParen, ")"));
                    i++;
                    continue;
                }

                throw new FormatException($"Unexpected character {c}");
            }

            return tokens;
        }

        private static string TokenizeNumber(string expression, ref int i)
        {
            StringBuilder sb = new();

            while (i < expression.Length &&
                   (char.IsDigit(expression[i]) || expression[i] == '.'))
            {
                sb.Append(expression[i]);
                i++;
            }

            return sb.ToString();
        }
    }
}

