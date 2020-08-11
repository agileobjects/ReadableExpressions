namespace AgileObjects.ReadableExpressions.Build.SourceCode
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
            Expression body)
        {
            Name = name;
            Definition = body.ToLambdaExpression();
            _summaryLines = summary.SplitToLines();
        }

        public string Name { get; }

        public LambdaExpression Definition { get; }

        public MethodExpression Build(
            ClassExpression parent,
            SourceCodeTranslationSettings settings)
        {
            return MethodExpression
                .For(parent, Name, _summaryLines, Definition, settings);
        }
    }
}