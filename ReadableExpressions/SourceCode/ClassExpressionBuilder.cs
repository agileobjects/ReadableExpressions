namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Api;
    using Extensions;

    internal class ClassExpressionBuilder : IClassExpressionSettings
    {
        private readonly IList<MethodExpressionBuilder> _methodBuilders;
        private readonly IList<string> _summaryLines;

        public ClassExpressionBuilder()
        {
            _methodBuilders = new List<MethodExpressionBuilder>();
            _summaryLines = Enumerable<string>.EmptyArray;
        }

        IClassExpressionSettings IClassExpressionSettings.WithMethod(LambdaExpression definition)
        {
            _methodBuilders.Add(new MethodExpressionBuilder(definition));
            return this;
        }

        public ClassExpression Build(
            SourceCodeExpression parent,
            TranslationSettings settings)
        {
            return new ClassExpression(
                parent,
                _summaryLines,
                _methodBuilders,
                settings);
        }
    }
}