namespace Polyhedral.FastFormula.AST
{
    public class DivisionASTNode : ASTNode
    {
        ASTNode a, b;
        public override float Eval(EnvironmentalConditions env)
        {
            // Division per 0 will return infinity values
            return a.Eval(env) / b.Eval(env);
        }

        /// <summary>
        /// This AST node evaluates to: nodeA / nodeB
        /// </summary>
        public DivisionASTNode(ASTNode nodeA, ASTNode nodeB)
        {
            a = nodeA;
            b = nodeB;
        }
    }
}
