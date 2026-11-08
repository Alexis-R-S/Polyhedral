namespace Polyhedral.FastFormula.ShuntingYard
{
    public class FormulaToken
    {
        public TokenType Type { get; private set; }
        public string Value { get; private set; }

        public int NumberArgs {get; set;}

        public FormulaToken(TokenType type, string value)
        {
            Type = type;
            Value = value;
            NumberArgs = 0;
        }

        public override string ToString() => NumberArgs == 0 ? Value : $"{Value}({NumberArgs})";
    }
}
