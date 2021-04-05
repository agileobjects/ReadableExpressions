﻿namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Extensions;

    /// <summary>
    /// An <see cref="ITranslatable"/> for a method signature, including accessibility, scope,
    /// and constructor parameters.
    /// </summary>
    public class ConstructorDefinitionTranslation : ITranslation
    {
        private readonly string _accessibility;
        private readonly ITranslation _typeNameTranslation;
        private readonly ITranslatable _parametersTranslation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorDefinitionTranslation"/> class
        /// for the given <paramref name="ctorInfo"/>.
        /// </summary>
        /// <param name="ctorInfo">
        /// The ConstructorInfo for which to create the
        /// <see cref="ConstructorDefinitionTranslation"/>.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public ConstructorDefinitionTranslation(
            ConstructorInfo ctorInfo,
            TranslationSettings settings)
            : this(new BclCtorInfoWrapper(ctorInfo), settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorDefinitionTranslation"/> class
        /// for the given <paramref name="ctor"/>.
        /// </summary>
        /// <param name="ctor">
        /// The <see cref="IConstructor"/> describing the ConstructorInfo for which to create the
        /// <see cref="ConstructorDefinitionTranslation"/>.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public ConstructorDefinitionTranslation(
            IConstructor ctor,
            TranslationSettings settings)
        {
            _accessibility = ctor.GetAccessibilityForTranslation();
            _typeNameTranslation = new TypeNameTranslation(ctor.DeclaringType, settings);
            _parametersTranslation = new ParameterSetDefinitionTranslation(ctor, settings);

            TranslationSize =
                _typeNameTranslation.TranslationSize +
                _parametersTranslation.TranslationSize;

            FormattingSize =
                settings.GetKeywordFormattingSize() + // <- for modifiers
                _typeNameTranslation.FormattingSize +
                _parametersTranslation.FormattingSize;
        }

        /// <inheritdoc />
        public ExpressionType NodeType => ExpressionType.New;

        /// <inheritdoc />
        public Type Type => _typeNameTranslation.Type;

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize() => _parametersTranslation.GetIndentSize();

        /// <inheritdoc />
        public int GetLineCount() => _parametersTranslation.GetLineCount() + 1;

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_accessibility);

            _typeNameTranslation.WriteTo(writer);
            _parametersTranslation.WriteTo(writer);
        }
    }
}
