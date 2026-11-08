using System;
using System.Collections.Generic;

namespace Polyhedral.FastFormula.ShuntingYard
{
    public static class RpnConverter
    {
        /// <summary>
        /// Value stored in <see cref="FormulaToken.NumberArgs"/> for an operator token used as a prefix (unary) operator.
        /// </summary>
        public const int UnaryArgumentCount = 1;

        /// <summary>
        /// Value stored in <see cref="FormulaToken.NumberArgs"/> for an operator token used as an infix (binary) operator.
        /// </summary>
        public const int BinaryArgumentCount = 2;

        // Unary minus binds tighter than * and / but looser than ^, so that -2^2 == -(2^2).
        private const int UnaryPrecedence = 4;

        // Higher precedence means higher priority.
        private static readonly Dictionary<string, int> BinaryPrecedence =
            new Dictionary<string, int>
            {
                { "=", 1 },
                { ">", 1 },
                { "<", 1 },
                { "+", 2 },
                { "-", 2 },
                { "*", 3 },
                { "/", 3 },
                { "^", 5 }
            };

        // Operators that group from the right: 2^3^2 == 2^(3^2)
        private static readonly HashSet<string> RightAssociative = new() { "^" };

        /// <summary>
        /// State kept for every open parenthesis.
        /// </summary>
        private sealed class ParenFrame
        {
            public readonly bool IsCall;   // true for "f(", false for a plain "("
            public int Commas;             // argument separators seen so far (calls only)

            public ParenFrame(bool isCall) => IsCall = isCall;
        }

        /// <summary>
        /// Converts infix tokens to Reverse Polish Notation (RPN) using the shunting-yard algorithm.
        /// Structural problems (missing operands, empty arguments, mismatched parentheses...) are
        /// reported as <see cref="FormatException"/> with the offending token and its index.
        /// </summary>
        /// <remarks>
        /// Side effect: sets <see cref="FormulaToken.NumberArgs"/> on operator and function tokens.
        /// </remarks>
        public static List<FormulaToken> ToRpn(IReadOnlyList<FormulaToken> tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            if (tokens.Count == 0) throw new FormatException("Empty expression");

            var output = new List<FormulaToken>(tokens.Count);
            var operators = new Stack<FormulaToken>();
            var frames = new Stack<ParenFrame>();

            // True whenever the grammar requires an operand (or prefix operator / '(' / function) next:
            // at the start, after an operator, after '(' and after ','.
            bool expectOperand = true;
            FormulaToken previous = null;

            for (int i = 0; i < tokens.Count; i++)
            {
                FormulaToken token = tokens[i];

                // A function name must be immediately followed by '('.
                if (previous != null && previous.Type == TokenType.Function && token.Type != TokenType.LeftParen)
                {
                    throw Error($"Function {previous.Value} is missing its argument list", token, i);
                }

                switch (token.Type)
                {
                    case TokenType.Number:
                    case TokenType.Variable:
                        if (!expectOperand) throw Error("Unexpected operand", token, i);
                        output.Add(token);
                        expectOperand = false;
                        break;

                    case TokenType.Operator:
                        if (expectOperand)
                        {
                            // Prefix position: only unary minus / plus make sense.
                            if (token.Value == "-")
                            {
                                token.NumberArgs = UnaryArgumentCount;
                                operators.Push(token); // prefix operators never pop anything
                            }
                            else if (token.Value != "+")
                            {
                                throw Error("Operator is missing its left operand", token, i);
                            }
                            // unary plus is a no-op: drop it
                        }
                        else
                        {
                            if (!BinaryPrecedence.TryGetValue(token.Value, out int curPrec))
                            {
                                throw Error("Unknown operator", token, i);
                            }

                            token.NumberArgs = BinaryArgumentCount;
                            bool rightAssoc = RightAssociative.Contains(token.Value);

                            while (operators.Count > 0 && operators.Peek().Type == TokenType.Operator)
                            {
                                int topPrec = PrecedenceOf(operators.Peek());
                                bool shouldPop = rightAssoc ? topPrec > curPrec : topPrec >= curPrec;
                                if (!shouldPop) break;
                                output.Add(operators.Pop());
                            }

                            operators.Push(token);
                            expectOperand = true;
                        }
                        break;

                    case TokenType.Function:
                        if (!expectOperand) throw Error("Unexpected function", token, i);
                        operators.Push(token);
                        break; // still expecting '('

                    case TokenType.LeftParen:
                        if (!expectOperand) throw Error("Unexpected '('", token, i);
                        bool isCall = previous != null && previous.Type == TokenType.Function;
                        operators.Push(token);
                        frames.Push(new ParenFrame(isCall));
                        break; // still expecting an operand (or ')' for an empty call)

                    case TokenType.RightParen:
                    {
                        if (frames.Count == 0) throw Error("Mismatched parentheses", token, i);

                        ParenFrame frame = frames.Peek();
                        bool emptyCall = false;
                        if (expectOperand)
                        {
                            // Only legal as "f()".
                            emptyCall = frame.IsCall && previous.Type == TokenType.LeftParen;
                            if (!emptyCall)
                            {
                                throw Error(previous.Type == TokenType.LeftParen
                                    ? "Parentheses encompass empty expression"
                                    : previous.Type == TokenType.Comma
                                        ? "Empty function argument"
                                        : "Missing operand", token, i);
                            }
                        }

                        PopUntilLeftParen(operators, output, token, i);
                        operators.Pop(); // remove '('
                        frames.Pop();

                        if (frame.IsCall)
                        {
                            FormulaToken function = operators.Pop();
                            function.NumberArgs = emptyCall ? 0 : frame.Commas + 1;
                            output.Add(function);
                        }

                        expectOperand = false;
                        break;
                    }

                    case TokenType.Comma:
                        if (frames.Count == 0 || !frames.Peek().IsCall)
                        {
                            throw Error("Misplaced argument separator", token, i);
                        }
                        if (expectOperand) throw Error("Empty function argument", token, i);

                        PopUntilLeftParen(operators, output, token, i);
                        frames.Peek().Commas++;
                        expectOperand = true;
                        break;

                    default:
                        throw Error($"Unhandled token type {token.Type}", token, i);
                }

                previous = token;
            }

            if (frames.Count > 0)
            {
                throw new FormatException("Mismatched parentheses: missing ')'");
            }
            if (expectOperand)
            {
                throw new FormatException(
                    $"Expression ends unexpectedly after '{previous.Value}' (missing operand or argument list)");
            }

            // Only operators can be left at this point.
            while (operators.Count > 0)
            {
                output.Add(operators.Pop());
            }

            return output;
        }

        private static int PrecedenceOf(FormulaToken op)
        {
            return op.NumberArgs == UnaryArgumentCount ? UnaryPrecedence : BinaryPrecedence[op.Value];
        }

        private static void PopUntilLeftParen(
            Stack<FormulaToken> operators, List<FormulaToken> output, FormulaToken token, int index)
        {
            while (operators.Count > 0 && operators.Peek().Type != TokenType.LeftParen)
            {
                output.Add(operators.Pop());
            }

            if (operators.Count == 0) throw Error("Mismatched parentheses", token, index);
        }

        private static FormatException Error(string message, FormulaToken token, int index)
        {
            return new FormatException($"{message} (token '{token.Value}' at index {index})");
        }
    }
}