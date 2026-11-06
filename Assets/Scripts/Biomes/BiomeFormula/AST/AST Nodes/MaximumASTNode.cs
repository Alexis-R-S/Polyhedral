using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Polyhedral.FastFormula.AST
{
    public class MaximumASTNode : ASTNode
    {
        IEnumerable<ASTNode> nodes;

        public override float Eval(EnvironmentalConditions env)
        {
            return nodes.Max((ASTNode node) => node.Eval(env));
        }

        /// <summary>
        /// This AST node evaluates to: max(nodeA, nodeB)
        /// </summary>
        public MaximumASTNode(IEnumerable<ASTNode> nodes)
        {
            if (!nodes.Any())
            {
                throw new FormatException("'Maximum' function requires at least 1 argument");
            }
            this.nodes = nodes;
        }
    }
}
