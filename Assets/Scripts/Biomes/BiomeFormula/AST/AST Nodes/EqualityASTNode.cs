namespace Polyhedral.FastFormula.AST
{
    public class EqualityASTNode : ASTNode
    {
        ASTNode a, b;
        public override float Eval(EnvironmentalConditions env)
        {
            return a.Eval(env) == b.Eval(env) ? 1f : 0f;
        }

        /// <summary>
        /// This AST node evaluates to: 1 if nodeA==nodeB, else 0
        /// </summary>
        public EqualityASTNode(ASTNode nodeA, ASTNode nodeB)
        {
            a = nodeA;
            b = nodeB;
        }
    }
}
