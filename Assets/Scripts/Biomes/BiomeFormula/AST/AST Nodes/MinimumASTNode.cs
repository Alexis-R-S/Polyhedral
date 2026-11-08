using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Polyhedral.FastFormula.AST
{
    public class MinimumASTNode : ASTNode
    {
        IEnumerable<ASTNode> nodes;

        public override float Eval(EnvironmentalConditions env)
        {
            return nodes.Min((ASTNode node) => node.Eval(env));
        }

        /// <summary>
        /// This AST node evaluates to: min(nodeA, nodeB)
        /// </summary>
        public MinimumASTNode(IEnumerable<ASTNode> nodes)
        {
            if (!nodes.Any())
            {
                throw new FormatException("'Minimum' function requires at least 1 argument");
            }
            this.nodes = nodes;
        }
    }
}