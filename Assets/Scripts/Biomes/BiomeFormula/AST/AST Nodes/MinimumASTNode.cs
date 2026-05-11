using UnityEngine;

namespace Polyhedral.FastFormula.AST
{
    public class MinimumASTNode : ASTNode
    {
        ASTNode a, b;

        public override float Eval(EnvironmentalConditions env)
        {
            return Mathf.Min(a.Eval(env), b.Eval(env));
        }

        /// <summary>
        /// This AST node evaluates to: min(nodeA, nodeB)
        /// </summary>
        public MinimumASTNode(ASTNode nodeA, ASTNode nodeB)
        {
            a = nodeA;
            b = nodeB;
        }
    }
}