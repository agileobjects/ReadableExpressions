namespace AgileObjects.ReadableExpressions.Build.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api;

    internal class SourceCodeExpressionBuilder :
        SourceCodeTranslationSettings,
        ISourceCodeExpressionSettings
    {
        private readonly IList<ClassExpressionBuilder> _classBuilders;

        public SourceCodeExpressionBuilder()
        {
            SetDefaultSourceCodeOptions(this);

            _classBuilders = new List<ClassExpressionBuilder>();
        }

        ISourceCodeExpressionSettings ISourceCodeExpressionSettings.WithNamespaceOf<T>()
            => SetNamespaceTo(typeof(T).Namespace);

        ISourceCodeExpressionSettings ISourceCodeExpressionSettings.WithNamespaceOf(Type type)
            => SetNamespaceTo(type.Namespace);

        ISourceCodeExpressionSettings ISourceCodeExpressionSettings.WithNamespace(string @namespace)
            => SetNamespaceTo(@namespace);

        private ISourceCodeExpressionSettings SetNamespaceTo(string @namespace)
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

            builder.Validate();
            _classBuilders.Add(builder);
            return this;
        }

        public SourceCodeExpression Build()
        {
            if (!_classBuilders.Any())
            {
                throw new InvalidOperationException(
                    "At least one class must be specified");
            }

            return new SourceCodeExpression(_classBuilders, this);
        }
    }
}