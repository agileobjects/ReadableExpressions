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

        public ClassExpressionBuilder(string name, string summary)
        {
            Name = name;
            _methodBuilders = new List<MethodExpressionBuilder>();
            _summaryLines = summary.SplitToLines();
        }

        public string Name { get; }

        IClassExpressionSettings IClassExpressionSettings.WithMethod(Expression body)
            => AddMethod(name: null, summary: null, body);

        IClassExpressionSettings IClassExpressionSettings.WithMethod(string name,
            Expression body)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Null or blank method name supplied");
            }

            return AddMethod(name, summary: null, body);
        }

        IClassExpressionSettings IClassExpressionSettings.WithMethod(string name,
            string summary,
            Expression body)
        {
            if (string.IsNullOrEmpty(summary))
            {
                throw new ArgumentException("Null or blank method summary supplied");
            }

            return AddMethod(name, summary, body);
        }

        private IClassExpressionSettings AddMethod(string name, string summary, Expression body)
        {
            if ((name != null) && _methodBuilders.Any(mb => mb.Name == name))
            {
                throw new InvalidOperationException(
                    $"Duplicate method name '{name}' specified.");
            }

            _methodBuilders.Add(new MethodExpressionBuilder(name, summary, body));
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