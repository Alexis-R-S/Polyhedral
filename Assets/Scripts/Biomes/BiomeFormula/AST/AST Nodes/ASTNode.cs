namespace Polyhedral.FastFormula.AST
{
    /// <summary>
    /// An AST node (stands for Abstract Syntax tree) is a tree structure
    /// that represents a calculation expression.
    /// </summary>
    public abstract class ASTNode
    {
        public abstract float Eval(EnvironmentalConditions env);
    }
}
