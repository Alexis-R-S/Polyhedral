using System;

namespace Polyhedral.FastFormula.AST
{
    public class VariableASTNode : ASTNode
    {
        Func<EnvironmentalConditions, float> getter;
        public override float Eval(EnvironmentalConditions env)
        {
            return getter(env);
        }

        /// <summary>
        /// This AST node evaluates to: variableGetter()
        /// </summary>
        public VariableASTNode(Func<EnvironmentalConditions, float> variableGetter)
        {
            getter = variableGetter;
        }
    }
}