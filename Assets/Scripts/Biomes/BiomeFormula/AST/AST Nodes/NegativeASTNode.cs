namespace Polyhedral.FastFormula.AST
{
    public class NegativeASTNode : ASTNode
    {
        ASTNode posNode;

        public override float Eval(EnvironmentalConditions env)
        {
            return - posNode.Eval(env);
        }

        /// <summary>
        /// This AST node evaluates to: -posNode
        /// </summary>
        public NegativeASTNode(ASTNode posNode)
        {
            this.posNode = posNode;
        }
    }
}
