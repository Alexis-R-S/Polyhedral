using System;
using System.Collections.Generic;

namespace Polyhedral.FastFormula.ShuntingYard
{
    public static class RpnConverter
    {
        // Precedence indicates operation priority, higher precedence means higher priority
        private static readonly Dictionary<string, int> Precedence =
            new Dictionary<string, int>
            {
                { "+", 1 },
                { "-", 1 },
                { "*", 2 },
                { "/", 2 },
                { "^", 3 }
            };

        /// <summary>
        /// Rpn stands for Reverse Polish Notation
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static List<FormulaToken> ToRpn(List<FormulaToken> tokens)
        {
            List<FormulaToken> output = new();
            Stack<FormulaToken> operators = new();

            foreach (FormulaToken token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        output.Add(token);
                        break;
                    
                    case TokenType.Operator:
                        while (operators.Count > 0 &&
                            operators.Peek().Type == TokenType.Operator &&
                            Precedence[operators.Peek().Value] >= Precedence[token.Value])
                        {
                            output.Add(operators.Pop());
                        }

                        operators.Push(token);
                        break;

                    case TokenType.LeftParen:
                        operators.Push(token);
                        break;

                    case TokenType.RightParen:
                        while (operators.Count > 0 &&
                            operators.Peek().Type != TokenType.LeftParen)
                        {
                            output.Add(operators.Pop());
                        }

                        if (operators.Count == 0)
                            throw new FormatException("Mismatched parentheses");

                        operators.Pop(); // remove '('
                        break;
                }
            }

            while (operators.Count > 0)
            {
                if (operators.Peek().Type == TokenType.LeftParen)
                    throw new FormatException("Mismatched parentheses");

                output.Add(operators.Pop());
            }

            return output;
        }
    }
}