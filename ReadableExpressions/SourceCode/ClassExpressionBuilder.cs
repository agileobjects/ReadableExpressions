namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public ClassExpressionBuilder(string name)
        {
            Name = name;
            _methodBuilders = new List<MethodExpressionBuilder>();
            _summaryLines = Enumerable<string>.EmptyArray;
        }

        public string Name { get; }

        IClassExpressionSettings IClassExpressionSettings.WithMethod(LambdaExpression definition)
            => ((IClassExpressionSettings)this).WithMethod(null, definition);

        IClassExpressionSettings IClassExpressionSettings.WithMethod(
            string name,
            LambdaExpression definition)
        {
            if ((name != null) && _methodBuilders.Any(mb => mb.Name == name))
            {
                throw new InvalidOperationException(
                    $"Duplicate method name '{name}' specified.");
            }

            _methodBuilders.Add(new MethodExpressionBuilder(name, definition));
            return this;
        }

        public ClassExpression Build(
            SourceCodeExpression parent,
            TranslationSettings settings)
        {
            return new ClassExpression(
                parent,
                Name,
                _summaryLines,
                _methodBuilders,
                settings);
        }
    }
}