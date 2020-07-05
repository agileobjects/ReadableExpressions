namespace AgileObjects.ReadableExpressions.SourceCode
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MethodExpressionBuilder
    {
        private readonly string _name;

        public MethodExpressionBuilder(
            string name,
            LambdaExpression definition)
        {
            _name = name;
            Definition = definition;
        }

        public LambdaExpression Definition { get; }

        public MethodExpression Build(
            ClassExpression parent,
            TranslationSettings settings)
        {
            return MethodExpression
                .For(parent, _name, Definition, settings);
        }
    }
}