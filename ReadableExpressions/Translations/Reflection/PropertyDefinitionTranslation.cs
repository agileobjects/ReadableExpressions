namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Extensions;
#if NETSTANDARD
    using NetStandardPolyfills;
#endif

    /// <summary>
    /// An <see cref="ITranslatable"/> for a property signature, including accessibility and scope.
    /// </summary>
    public class PropertyDefinitionTranslation : ITranslation
    {
        private readonly string _accessibility;
        private readonly string _modifiers;
        private readonly ITranslatable _declaringTypeNameTranslation;
        private readonly ITranslatable _propertyTypeNameTranslation;
        private readonly string _propertyName;
        private readonly ITranslatable[] _accessorTranslations;

        internal PropertyDefinitionTranslation(
            PropertyInfo property,
            TranslationSettings settings)
            : this(property, property.GetAccessors(nonPublic: true), settings)
        {
        }

        internal PropertyDefinitionTranslation(
            PropertyInfo property,
            MethodInfo accessor,
            TranslationSettings settings)
            : this(property, new[] { accessor }, settings)
        {
        }

        private PropertyDefinitionTranslation(
            PropertyInfo property,
            IList<MethodInfo> accessors,
            TranslationSettings settings)
            : this(
                new BclPropertyWrapper(property, settings),
                accessors.ProjectToArray<MethodInfo, IComplexMember>(acc =>
                    new BclMethodWrapper(acc, settings)),
                includeDeclaringType: true,
                settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDefinitionTranslation"/> class.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="includeDeclaringType"></param>
        /// <param name="settings"></param>
        public PropertyDefinitionTranslation(
            IProperty property,
            bool includeDeclaringType,
            TranslationSettings settings)
            : this(
                property,
                new List<IComplexMember>(property.GetAccessors()),
                includeDeclaringType,
                settings)
        {
        }

        private PropertyDefinitionTranslation(
            IProperty property,
            IList<IComplexMember> accessors,
            bool includeDeclaringType,
            TranslationSettings settings)
        {
            Type = property.Type;

            _accessibility = property.GetAccessibilityForTranslation();
            _modifiers = accessors[0].GetModifiersForTranslation();

            _propertyTypeNameTranslation =
                new TypeNameTranslation(property.Type, settings);

            _propertyName = property.Name;

            var translationSize =
                _accessibility.Length +
                _modifiers.Length +
                _propertyTypeNameTranslation.TranslationSize +
                _propertyName.Length;

            var keywordFormattingSize = settings.GetKeywordFormattingSize();

            var formattingSize =
                keywordFormattingSize + // <- For modifiers
                _propertyTypeNameTranslation.FormattingSize;

            if (includeDeclaringType && property.DeclaringType != null)
            {
                _declaringTypeNameTranslation =
                    new TypeNameTranslation(property.DeclaringType, settings);

                translationSize += _declaringTypeNameTranslation.TranslationSize + ".".Length;
                formattingSize += _declaringTypeNameTranslation.FormattingSize;
            }

            _accessorTranslations = new ITranslatable[accessors.Count];

            for (var i = 0; i < accessors.Count; ++i)
            {
                var accessorTranslation =
                    new PropertyAccessorDefinitionTranslation(this, accessors[i], settings);

                translationSize += accessorTranslation.TranslationSize;
                formattingSize += accessorTranslation.FormattingSize;

                _accessorTranslations[i] = accessorTranslation;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        /// <inheritdoc />
        public ExpressionType NodeType => ExpressionType.MemberAccess;

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize() => _propertyTypeNameTranslation.GetIndentSize();

        /// <inheritdoc />
        public int GetLineCount() => _propertyTypeNameTranslation.GetLineCount();

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_accessibility + _modifiers);

            _propertyTypeNameTranslation.WriteTo(writer);
            writer.WriteSpaceToTranslation();

            if (_declaringTypeNameTranslation != null)
            {
                _declaringTypeNameTranslation.WriteTo(writer);
                writer.WriteDotToTranslation();
            }

            writer.WriteToTranslation(_propertyName);
            writer.WriteToTranslation(" { ");

            foreach (var accessorTranslation in _accessorTranslations)
            {
                accessorTranslation.WriteTo(writer);
            }

            writer.WriteToTranslation("}");
        }

        private class PropertyAccessorDefinitionTranslation : ITranslatable
        {
            private readonly string _accessor;

            public PropertyAccessorDefinitionTranslation(
                PropertyDefinitionTranslation parent,
                IMember accessor,
                TranslationSettings settings)
            {
                _accessor = accessor.Type != typeof(void) ? "get" : "set";

                TranslationSize = _accessor.Length + "; ".Length;
                FormattingSize = settings.GetKeywordFormattingSize();

                var accessibility = accessor.GetAccessibilityForTranslation();

                if (accessibility == parent._accessibility)
                {
                    return;
                }

                _accessor = accessibility + _accessor;
                TranslationSize += accessibility.Length;
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize() => 0;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteKeywordToTranslation(_accessor);
                writer.WriteToTranslation("; ");
            }
        }
    }
}