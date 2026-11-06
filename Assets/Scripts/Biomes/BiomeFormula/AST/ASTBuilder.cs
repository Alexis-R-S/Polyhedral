using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Polyhedral.FastFormula.ShuntingYard;
using UnityEngine;

namespace Polyhedral.FastFormula.AST
{
    public class ASTBuilder
    {
        /// <summary>
        /// Convert a reverse polish notation to an AST
        /// </summary>
        /// <param name="tokens">Reverse polish notation tokens</param>
        /// <returns></returns>
        public ASTNode FromRPN(List<FormulaToken> tokens)
        {
            // If token is operand: push onto stack
            // If token is operator: pop X from stack, build, then push result to stack
            // Return pop stack

            if (tokens == null || tokens.Count == 0)
            {
                throw new ArgumentException("Tokens cannot be null or empty");
            }
            Stack<ASTNode> astStack = new Stack<ASTNode>();

            foreach (FormulaToken token in tokens)
            {
                astStack.Push(CreateNode(token, astStack));
            }
            
            return astStack.Pop();
        }

        /// <summary>
        /// Creates an AST node based on the given token and stack.
        /// </summary>
        private ASTNode CreateNode(FormulaToken token, Stack<ASTNode> astStack)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    return CreateConstantNode(token.Value);

                case TokenType.Variable:
                    return CreateVariableNode(token.Value);

                case TokenType.Operator:
                    return CreateOperatorNode(token.Value, astStack);

                case TokenType.Function:
                    return CreateFunctionNode(token, astStack);
                
                default:
                    throw new ArgumentException($"Unsupported AST token type {token.Type}");
            }
        }

        private ASTNode CreateConstantNode(string token)
        {
            if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedToken))
            {
                return new ConstantASTNode(parsedToken);
            }
            else
            {
                throw new FormatException($"Token {token} cannot be parsed into a float.");
            }
        }

        private ASTNode CreateVariableNode(string token)
        {
            if (EnvironmentalConditions.getGetter(token, out Func<EnvironmentalConditions, float> envVarGetter))
            {
                return new VariableASTNode(envVarGetter);
            }
            else
            {
                throw new ArgumentException($"Unrecognized variable token {token}");
            }
        }

        private ASTNode CreateOperatorNode(string token, Stack<ASTNode> astStack)
        {
            ASTNode tempNodeA, tempNodeB;
            switch (token)
            {
                case "+":
                    return new AdditionASTNode(astStack.Pop(), astStack.Pop());

                case "-":
                    tempNodeA = astStack.Pop();
                    if (astStack.TryPop(out tempNodeB))
                    {
                        return new SubstractionASTNode(tempNodeB, tempNodeA);
                    }
                    else
                    {
                        return new NegativeASTNode(tempNodeA);
                    }

                case "*":
                    return new MultiplicationASTNode(astStack.Pop(), astStack.Pop());

                case "/":
                    tempNodeA = astStack.Pop();
                    return new DivisionASTNode(astStack.Pop(), tempNodeA);

                case ">":
                    tempNodeA = astStack.Pop();
                    return new GreaterThanASTNode(astStack.Pop(), tempNodeA);

                case "<":
                    tempNodeA = astStack.Pop();
                    return new GreaterThanASTNode(tempNodeA, astStack.Pop());

                case "=":
                    tempNodeA = astStack.Pop();
                    return new EqualityASTNode(astStack.Pop(), tempNodeA);

                case "^":
                    tempNodeA = astStack.Pop();
                    return new ExponentASTNode(astStack.Pop(), tempNodeA);

                default:
                    throw new ArgumentException($"Unrecognized token {token}");
            }
        }

        private ASTNode CreateFunctionNode(FormulaToken token, Stack<ASTNode> astStack)
        {
            var functionArguments = new ASTNode[token.NumberArgs];
            for (int i = functionArguments.Length - 1; i >= 0; i--)
            {
                functionArguments[i] = astStack.Pop();
            }

            switch (token.Value)
            {
                case "max":
                    return new MaximumASTNode(functionArguments);

                case "min":
                    return new MinimumASTNode(functionArguments);
                
                default:
                    throw new ArgumentException($"Unrecognized function {token.Value}");
            }
        }
    }
}