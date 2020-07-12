namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class MethodExpressionBuilder
    {
        private readonly IList<string> _summaryLines;

        public MethodExpressionBuilder(
            string name,
            string summary,
            LambdaExpression definition)
        {
            Name = name;
            Definition = definition;
            _summaryLines = summary.SplitToLines();
        }

        public string Name { get; }

        public LambdaExpression Definition { get; }

        public MethodExpression Build(
            ClassExpression parent,
            TranslationSettings settings)
        {
            return MethodExpression
                .For(parent, Name, _summaryLines, Definition, settings);
        }
    }
}