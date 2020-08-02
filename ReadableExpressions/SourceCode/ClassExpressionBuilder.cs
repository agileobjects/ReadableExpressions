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
        private IList<Type> _interfaceTypes;

        public ClassExpressionBuilder(string name, string summary)
        {
            Name = name;
            _methodBuilders = new List<MethodExpressionBuilder>();
            _summaryLines = summary.SplitToLines();
        }

        public string Name { get; }

        public IClassExpressionSettings Implementing<TInterface>() where TInterface : class
            => AddInterface(typeof(TInterface));

        public IClassExpressionSettings Implementing(Type @interface)
            => AddInterface(@interface);

        private ClassExpressionBuilder AddInterface(Type interfaceType)
        {
            _interfaceTypes ??= new List<Type>();

            if (!_interfaceTypes.Contains(interfaceType))
            {
                _interfaceTypes.Add(interfaceType);
            }

            return this;
        }

        IClassExpressionSettings IClassExpressionSettings.WithMethod(Expression body)
            => AddMethod(name: null, summary: null, body, allowNullName: true);

        IClassExpressionSettings IClassExpressionSettings.WithMethod(
            string name,
            Expression body)
        {
            return AddMethod(name, summary: null, body);
        }

        IClassExpressionSettings IClassExpressionSettings.WithMethod(
            string name,
            string summary,
            Expression body)
        {
            return AddMethod(name, summary, body);
        }

        private IClassExpressionSettings AddMethod(
            string name,
            string summary,
            Expression body,
            bool allowNullName = false)
        {
            name.ThrowIfInvalidName<ArgumentException>("Method", allowNullName);

            if (!allowNullName && _methodBuilders.Any(mb => mb.Name == name))
            {
                throw new InvalidOperationException(
                    $"Duplicate method name '{name}' specified.");
            }

            _methodBuilders.Add(new MethodExpressionBuilder(name, summary, body));
            return this;
        }

        public void Validate()
        {
            if (_methodBuilders.Any())
            {
                return;
            }

            var name = Name != null ? $" for class '{Name}'" : null;

            throw new InvalidOperationException(
                "At least one method must be specified" + name);
        }

        public ClassExpression Build(
            SourceCodeExpression parent,
            TranslationSettings settings)
        {
            return new ClassExpression(
                parent,
                Name,
                _interfaceTypes,
                _summaryLines,
                _methodBuilders,
                settings);
        }
    }
}