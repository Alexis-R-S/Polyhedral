using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Polyhedral.FastFormula.AST;
using UnityEngine;

public class FunctionMapper
{
    private static readonly Dictionary<string, Func<List<ASTNode>, ASTNode>> astNodeMapper = new()
    {
        ["min"] = (List<ASTNode> arguments) => new MinimumASTNode(arguments),
        ["max"] = (List<ASTNode> arguments) => new MaximumASTNode(arguments),
    };

    public static bool getFunctionNode(string functionToken, List<ASTNode> nodes, out ASTNode node)
    {
        if (astNodeMapper.TryGetValue(functionToken, out Func<List<ASTNode>, ASTNode> astNodeCreationFunc))
        {
            node = astNodeCreationFunc(nodes);
            return true;
        }
        node = null;
        return false;
    }

    public static bool hasFunction(string functionToken)
    {
        return astNodeMapper.ContainsKey(functionToken);
    }
}
