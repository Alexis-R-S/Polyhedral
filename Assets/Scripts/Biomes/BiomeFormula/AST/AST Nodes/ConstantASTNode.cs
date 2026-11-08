namespace Polyhedral.FastFormula.AST
{
    public class ConstantASTNode : ASTNode
    {
        float value;
        public override float Eval(EnvironmentalConditions env)
        {
            return value;
        }

        /// <summary>
        /// This AST node evaluates to: value
        /// </summary>
        public ConstantASTNode(float value)
        {
            this.value = value;
        }
    }
}
