namespace Polyhedral.FastFormula.ShuntingYard
{
    public class FormulaToken
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }

        public FormulaToken(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => Value;
    }
}
