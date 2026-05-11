namespace Polyhedral.FastFormula.AST
{
    public class SubstractionASTNode : ASTNode
    {
        ASTNode a, b;

        public override float Eval(EnvironmentalConditions env)
        {
            return a.Eval(env) - b.Eval(env);
        }

        /// <summary>
        /// This AST node evaluates to: nodeA - nodeB
        /// </summary>
        public SubstractionASTNode(ASTNode nodeA, ASTNode nodeB)
        {
            a = nodeA;
            b = nodeB;
        }
    }
}
