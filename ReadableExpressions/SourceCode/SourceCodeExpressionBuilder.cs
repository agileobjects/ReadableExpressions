namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.Generic;
    using Api;

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
            var builder = new ClassExpressionBuilder();
            configuration.Invoke(builder);

            _classBuilders.Add(builder);
            return this;
        }

        public SourceCodeExpression Build()
            => new SourceCodeExpression(_classBuilders, this);
    }
}