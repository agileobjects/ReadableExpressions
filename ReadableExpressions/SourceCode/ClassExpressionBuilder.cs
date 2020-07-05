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
        private string _name;
        private readonly IList<string> _summaryLines;

        public ClassExpressionBuilder()
        {
            _methodBuilders = new List<MethodExpressionBuilder>();
            _summaryLines = Enumerable<string>.EmptyArray;
        }

        IClassExpressionSettings IClassExpressionSettings.Named(string name)
        {
            _name = name;
            return this;
        }

        IClassExpressionSettings IClassExpressionSettings.WithMethod(LambdaExpression definition)
            => ((IClassExpressionSettings)this).WithMethod(null, definition);

        IClassExpressionSettings IClassExpressionSettings.WithMethod(
            string name,
            LambdaExpression definition)
        {
            _methodBuilders.Add(new MethodExpressionBuilder(name, definition));
            return this;
        }

        public ClassExpression Build(
            SourceCodeExpression parent,
            TranslationSettings settings)
        {
            return new ClassExpression(
                parent,
                _name,
                _summaryLines,
                _methodBuilders,
                settings);
        }
    }
}