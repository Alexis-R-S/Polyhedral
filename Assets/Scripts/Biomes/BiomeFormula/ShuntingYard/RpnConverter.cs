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
                { "=", 1 },
                { ">", 1 },
                { "<", 1 },
                { "+", 2 },
                { "-", 2 },
                { "*", 3 },
                { "/", 3 },
                { "^", 4 }
            };

        // RightAssociative indicates which operators are right associative, meaning they group from the right
        private static readonly HashSet<string> RightAssociative = new() { "^" };

        /// <summary>
        /// Rpn stands for Reverse Polish Notation
        /// </summary>
        public static List<FormulaToken> ToRpn(List<FormulaToken> tokens)
        {
            List<FormulaToken> output = new();
            Stack<FormulaToken> operators = new();
            Stack<bool> parenIsCall = new();    // Keeps track of whether the current parenthesis is a function call or not
            Stack<int> argCount = new();          // Keeps track of the number of arguments for each function call
            FormulaToken previous = null;

            foreach (FormulaToken token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                    case TokenType.Variable:
                        output.Add(token);
                        break;
                    
                    case TokenType.Operator:
                        while (operators.Count > 0 &&
                            operators.Peek().Type == TokenType.Operator)
                        {
                            int topPrec = Precedence[operators.Peek().Value];
                            int curPrec = Precedence[token.Value];
                            bool shouldPop = RightAssociative.Contains(token.Value)
                                ? topPrec > curPrec
                                : topPrec >= curPrec;
                            if (!shouldPop) break;
                            output.Add(operators.Pop());
                        }
                        operators.Push(token);
                        break;

                    case TokenType.LeftParen:
                        bool isCall = operators.Count > 0 && operators.Peek().Type == TokenType.Function;
                        operators.Push(token);
                        parenIsCall.Push(isCall);
                        if (isCall)
                        {
                            argCount.Push(0); // Start counting arguments for this function call
                        }
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

                        if (parenIsCall.Pop())
                        {
                            int commas = argCount.Pop();
                            bool empty = previous.Type == TokenType.LeftParen;
                            if (previous.Type == TokenType.Comma    // Prevent calls such as: f(1,)
                                || previous.Type == TokenType.Function
                            )
                            {
                                throw new FormatException("Empty function argument");
                            }

                            FormulaToken funcToken = operators.Pop();
                            funcToken.NumberArgs = empty ? 0 : commas + 1;
                            output.Add(funcToken);
                        }
                        else if (previous.Type == TokenType.LeftParen)
                        {
                            throw new FormatException("Parentheses encompass empty expression");
                        }
                        break;

                    case TokenType.Function:
                        operators.Push(token);
                        break;

                    case TokenType.Comma:
                        while (operators.Count > 0 && operators.Peek().Type != TokenType.LeftParen)
                        {
                            output.Add(operators.Pop());
                        }

                        if (operators.Count == 0) {
                            throw new FormatException("Mismatched parentheses"); 
                        }
                        if (parenIsCall.Count == 0 || !parenIsCall.Peek()) {
                            throw new FormatException("Misplaced argument separator");
                        }
                        if (previous ==null
                            || previous.Type == TokenType.LeftParen
                            || previous.Type == TokenType.Comma
                            || previous.Type == TokenType.Function    
                        )
                        {
                            throw new FormatException("Empty function argument");
                        }
                        argCount.Push(argCount.Pop() +1);
                        break;
                    default:
                        throw new FormatException($"Unhandled token type {token.Type}");
                }
                previous = token;
            }

            while (operators.Count > 0)
            {
                FormulaToken top = operators.Pop();

                if (top.Type == TokenType.LeftParen) {
                    throw new FormatException("Mismatched parentheses");
                }
                if (top.Type == TokenType.Function)
                {
                    throw new FormatException($"Function {top.Value} is missing its argument list.");
                }

                output.Add(top);
            }

            return output;
        }
    }
}