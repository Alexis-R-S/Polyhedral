using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polyhedral.FastFormula.AST;
using Polyhedral.FastFormula.ShuntingYard;

public class BiomeRule
{
    private static ASTBuilder astBuilder = new();
    private ASTNode scoreAst;
    private ASTNode[] preconditionsAsts;

    /// <summary>
    /// Create a new biome rule with the given preconditions and score calculation
    /// </summary>
    /// <param name="preconditionsRpn">Reverse polish notation to calculate preconditions</param>
    /// <param name="scoreRpn">Reverse polish notation to calculate score</param>
    public BiomeRule(string[] preconditionsChecks, string scoreMathFormula)
    {
        scoreAst = ConvertStringToAst(scoreMathFormula);
        preconditionsAsts = new ASTNode[preconditionsChecks.Length];
        for (int i=0; i<preconditionsChecks.Length; i++)
        {
            preconditionsAsts[i] = ConvertStringToAst(preconditionsChecks[i]);
        }
    }

    public BiomeRule(ASTNode[] preconditionsAsts, ASTNode scoreAst)
    {
        this.preconditionsAsts = preconditionsAsts;
        this.scoreAst = scoreAst;
    }

    /// <summary>
    /// Evaluate the biome score based on the given environmental conditions
    /// </summary>
    /// <param name="env">The environmental conditions</param>
    /// <returns>The evaluated score</returns>
    public float EvaluateScore(EnvironmentalConditions env)
    {
        foreach (ASTNode precondition in preconditionsAsts)
        {
            if (precondition.Eval(env) == 0)
            {
                return float.MinValue;
            }
        }
        
        return scoreAst.Eval(env);
    }

    private static ASTNode ConvertStringToAst(string stringFormula)
    {
        return astBuilder.FromRPN(
            RpnConverter.ToRpn(
                FormulaTokenizer.Tokenize(stringFormula)
            )
        );
    }
}
