using UnityEngine;

namespace Polyhedral.FastFormula.AST
{
    public class MaximumASTNode : ASTNode
    {
        ASTNode a, b;

        public override float Eval(EnvironmentalConditions env)
        {
            return Mathf.Max(a.Eval(env), b.Eval(env));
        }

        /// <summary>
        /// This AST node evaluates to: max(nodeA, nodeB)
        /// </summary>
        public MaximumASTNode(ASTNode nodeA, ASTNode nodeB)
        {
            a = nodeA;
            b = nodeB;
        }
    }
}
