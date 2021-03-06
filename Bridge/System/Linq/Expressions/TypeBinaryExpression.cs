using Bridge;

namespace System.Linq.Expressions
{
    [External]
    [Name("Object")]
    [Cast("{this}.ntype === 45 || {this}.ntype === 81")]
    public sealed class TypeBinaryExpression : Expression
    {
        [FieldProperty]
        public extern Expression Expression { get; private set; }

        [FieldProperty]
        public extern Type TypeOperand { get; private set; }

        internal extern TypeBinaryExpression();
    }
}