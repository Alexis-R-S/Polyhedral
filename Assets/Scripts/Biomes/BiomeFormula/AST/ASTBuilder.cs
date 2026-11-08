using System;
using System.Collections.Generic;
using System.Globalization;
using Polyhedral.FastFormula.ShuntingYard;

namespace Polyhedral.FastFormula.AST
{
    public sealed class ASTBuilder
    {
        // Binary operators: (left, right) => node. Adding an operator is one line here
        // (plus its precedence in RpnConverter).
        private static readonly Dictionary<string, Func<ASTNode, ASTNode, ASTNode>> BinaryOperators =
            new Dictionary<string, Func<ASTNode, ASTNode, ASTNode>>
            {
                ["+"] = (l, r) => new AdditionASTNode(l, r),
                ["-"] = (l, r) => new SubstractionASTNode(l, r),
                ["*"] = (l, r) => new MultiplicationASTNode(l, r),
                ["/"] = (l, r) => new DivisionASTNode(l, r),
                [">"] = (l, r) => new GreaterThanASTNode(l, r),
                ["<"] = (l, r) => new GreaterThanASTNode(r, l), // a < b  ==  b > a
                ["="] = (l, r) => new EqualityASTNode(l, r),
                ["^"] = (l, r) => new ExponentASTNode(l, r),
            };

        private sealed class FunctionSpec
        {
            public readonly int MinArgs;
            public readonly int MaxArgs;
            public readonly Func<ASTNode[], ASTNode> Create;

            public FunctionSpec(int minArgs, int maxArgs, Func<ASTNode[], ASTNode> create)
            {
                MinArgs = minArgs;
                MaxArgs = maxArgs;
                Create = create;
            }
        }

        private static readonly Dictionary<string, FunctionSpec> Functions =
            new Dictionary<string, FunctionSpec>
            {
                ["max"] = new FunctionSpec(1, int.MaxValue, args => new MaximumASTNode(args)),
                ["min"] = new FunctionSpec(1, int.MaxValue, args => new MinimumASTNode(args)),
            };

        /// <summary>
        /// Converts Reverse Polish Notation tokens (as produced by <see cref="RpnConverter.ToRpn"/>) to an AST.
        /// </summary>
        /// <exception cref="ArgumentException">Tokens are null/empty, or a variable is unknown.</exception>
        /// <exception cref="FormatException">The RPN is malformed (missing or leftover operands, bad arity...).</exception>
        public ASTNode FromRPN(IReadOnlyList<FormulaToken> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                throw new ArgumentException("Tokens cannot be null or empty");
            }

            var astStack = new Stack<ASTNode>();

            foreach (FormulaToken token in tokens)
            {
                astStack.Push(CreateNode(token, astStack));
            }

            // A well-formed expression reduces to exactly one node.
            if (astStack.Count != 1)
            {
                throw new FormatException(
                    $"Malformed expression: {astStack.Count} values left after evaluation, expected 1");
            }

            return astStack.Pop();
        }

        private ASTNode CreateNode(FormulaToken token, Stack<ASTNode> astStack)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    return CreateConstantNode(token.Value);

                case TokenType.Variable:
                    return CreateVariableNode(token.Value);

                case TokenType.Operator:
                    return CreateOperatorNode(token, astStack);

                case TokenType.Function:
                    return CreateFunctionNode(token, astStack);

                default:
                    throw new ArgumentException($"Unsupported AST token type {token.Type}");
            }
        }

        private static ASTNode CreateConstantNode(string token)
        {
            if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                return new ConstantASTNode(value);
            }

            throw new FormatException($"Token {token} cannot be parsed into a float.");
        }

        private static ASTNode CreateVariableNode(string token)
        {
            if (EnvironmentalConditions.getGetter(token, out Func<EnvironmentalConditions, float> getter))
            {
                return new VariableASTNode(getter);
            }

            throw new ArgumentException($"Unrecognized variable token {token}");
        }

        private static ASTNode CreateOperatorNode(FormulaToken token, Stack<ASTNode> astStack)
        {
            // Unary operators are explicitly marked by the RPN converter; no guessing from stack contents.
            if (token.NumberArgs == RpnConverter.UnaryArgumentCount)
            {
                if (token.Value != "-")
                {
                    throw new ArgumentException($"Unrecognized unary operator {token.Value}");
                }

                return new NegativeASTNode(PopOperand(astStack, token));
            }

            if (!BinaryOperators.TryGetValue(token.Value, out var factory))
            {
                throw new ArgumentException($"Unrecognized operator {token.Value}");
            }

            // Operands come off the stack in reverse order: right first, then left.
            ASTNode right = PopOperand(astStack, token);
            ASTNode left = PopOperand(astStack, token);
            return factory(left, right);
        }

        private static ASTNode CreateFunctionNode(FormulaToken token, Stack<ASTNode> astStack)
        {
            if (!Functions.TryGetValue(token.Value, out FunctionSpec spec))
            {
                throw new ArgumentException($"Unrecognized function {token.Value}");
            }

            int count = token.NumberArgs;
            if (count < spec.MinArgs || count > spec.MaxArgs)
            {
                string expected = spec.MaxArgs == int.MaxValue
                    ? $"at least {spec.MinArgs}"
                    : spec.MinArgs == spec.MaxArgs ? $"{spec.MinArgs}" : $"{spec.MinArgs} to {spec.MaxArgs}";
                throw new FormatException($"Function {token.Value} expects {expected} argument(s), got {count}");
            }

            var arguments = new ASTNode[count];
            for (int i = count - 1; i >= 0; i--)
            {
                arguments[i] = PopOperand(astStack, token);
            }

            return spec.Create(arguments);
        }

        private static ASTNode PopOperand(Stack<ASTNode> astStack, FormulaToken consumer)
        {
            if (astStack.Count == 0)
            {
                throw new FormatException($"Missing operand for '{consumer.Value}'");
            }

            return astStack.Pop();
        }
    }
}