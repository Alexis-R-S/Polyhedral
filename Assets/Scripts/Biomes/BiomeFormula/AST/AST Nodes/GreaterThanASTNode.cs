namespace Polyhedral.FastFormula.AST
{
    public class GreaterThanASTNode : ASTNode
    {
        ASTNode a, b;

        public override float Eval(EnvironmentalConditions env)
        {
            return a.Eval(env) > b.Eval(env) ? 1f : 0f;
        }

        /// <summary>
        /// This AST node evaluates to: 1 if nodeA > node B, else 0
        /// </summary>
        public GreaterThanASTNode(ASTNode nodeA, ASTNode nodeB)
        {
            a = nodeA;
            b = nodeB;
        }
    }
}
