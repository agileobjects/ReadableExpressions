namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class AssignmentExpressionTranslator : ExpressionTranslatorBase
    {
        public AssignmentExpressionTranslator()
            : base(ExpressionType.Assign)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var assignment = (BinaryExpression)expression;
            var target = translatorRegistry.Translate(assignment.Left);
            var value = translatorRegistry.Translate(assignment.Right);

            return $"var {target} = {value}";
        }
    }
}