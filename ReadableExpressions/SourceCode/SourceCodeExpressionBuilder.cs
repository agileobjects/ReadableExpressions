namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api;
    using Extensions;

    internal class SourceCodeExpressionBuilder :
        TranslationSettings,
        ISourceCodeExpressionSettings
    {
        private readonly IList<ClassExpressionBuilder> _classBuilders;

        public SourceCodeExpressionBuilder()
        {
            SetDefaultSourceCodeOptions(this);

            _classBuilders = new List<ClassExpressionBuilder>();
        }

        ISourceCodeExpressionSettings ISourceCodeExpressionSettings.WithNamespaceOf<T>()
        {
            SetNamespace(typeof(T).Namespace);
            return this;
        }

        ISourceCodeExpressionSettings ISourceCodeExpressionSettings.WithNamespace(string @namespace)
        {
            SetNamespace(@namespace);
            return this;
        }

        ISourceCodeExpressionSettings ISourceCodeExpressionSettings.WithClass(
            Func<IClassExpressionSettings, IClassExpressionSettings> configuration)
        {
            return AddClass(name: null, summary: null, configuration, allowNullName: true);
        }

        ISourceCodeExpressionSettings ISourceCodeExpressionSettings.WithClass(
            string name,
            Func<IClassExpressionSettings, IClassExpressionSettings> configuration)
        {
            return AddClass(name, summary: null, configuration);
        }

        ISourceCodeExpressionSettings ISourceCodeExpressionSettings.WithClass(
            string name,
            string summary,
            Func<IClassExpressionSettings, IClassExpressionSettings> configuration)
        {
            return AddClass(name, summary, configuration);
        }

        private ISourceCodeExpressionSettings AddClass(
            string name,
            string summary,
            Func<IClassExpressionSettings, IClassExpressionSettings> configuration,
            bool allowNullName = false)
        {
            name.ThrowIfInvalidName<ArgumentException>("Class", allowNullName);

            if (!allowNullName && _classBuilders.Any(b => b.Name == name))
            {
                throw new InvalidOperationException(
                    $"Duplicate class name '{name}' specified.");
            }

            var builder = new ClassExpressionBuilder(name, summary);
            configuration.Invoke(builder);

            _classBuilders.Add(builder);
            return this;
        }

        public SourceCodeExpression Build()
            => new SourceCodeExpression(_classBuilders, this);
    }
}