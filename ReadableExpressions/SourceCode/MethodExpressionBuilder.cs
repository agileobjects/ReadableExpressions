namespace AgileObjects.ReadableExpressions.SourceCode
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MethodExpressionBuilder
    {
        public MethodExpressionBuilder(
            string name,
            LambdaExpression definition)
        {
            Name = name;
            Definition = definition;
        }

        public string Name { get; }
        
        public LambdaExpression Definition { get; }

        public MethodExpression Build(
            ClassExpression parent,
            TranslationSettings settings)
        {
            return MethodExpression
                .For(parent, Name, Definition, settings);
        }
    }
}