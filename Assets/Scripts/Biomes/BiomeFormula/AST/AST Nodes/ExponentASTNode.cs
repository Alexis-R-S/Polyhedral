using System.Collections;
using System.Collections.Generic;
using Polyhedral.FastFormula.AST;
using UnityEngine;

public class ExponentASTNode : ASTNode
{
    private ASTNode baseNode;
    private ASTNode exponentNode;

    public ExponentASTNode(ASTNode baseNode, ASTNode exponentNode)
    {
        this.baseNode = baseNode;
        this.exponentNode = exponentNode;
    }

    public override float Eval(EnvironmentalConditions env)
    {
        return Mathf.Pow(baseNode.Eval(env), exponentNode.Eval(env));
    }
}
