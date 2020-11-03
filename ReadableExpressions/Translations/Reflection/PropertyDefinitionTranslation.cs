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

        private PropertyDefinitionTranslation(
            IProperty property,
            IList<IComplexMember> accessors,
            bool includeDeclaringType,
            TranslationSettings settings)
            : this(
                property,
                accessors,
                includeDeclaringType,
                (pd, acc, stg) => new PropertyAccessorDefinitionTranslation(pd, acc, stg),
                settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDefinitionTranslation"/> class.
        /// </summary>
        /// <param name="property">The <see cref="IProperty"/> to translate.</param>
        /// <param name="accessors">
        /// One or two <see cref="IComplexMember"/>s describing one or both of the
        /// <paramref name="property"/>'s accessor(s). 
        /// </param>
        /// <param name="includeDeclaringType">
        /// A value indicating whether to include the <paramref name="property"/>'s declaring type
        /// in the translation.
        /// </param>
        /// <param name="accessorTranslationFactory">
        /// A <see cref="PropertyAccessorTranslationFactory"/> with which to translate the
        /// <paramref name="property"/>'s accessor(s).
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        protected PropertyDefinitionTranslation(
            IProperty property,
            IList<IComplexMember> accessors,
            bool includeDeclaringType,
            PropertyAccessorTranslationFactory accessorTranslationFactory,
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
                    accessorTranslationFactory.Invoke(this, accessors[i], settings);

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
            WriteAccessorsStartTo(writer);

            foreach (var accessorTranslation in _accessorTranslations)
            {
                accessorTranslation.WriteTo(writer);
            }

            WriteAccessorsEndTo(writer);
        }

        /// <summary>
        /// Write characters to the given <paramref name="writer"/> to being translation of the
        /// property accessors.
        /// </summary>
        /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the characters.</param>
        protected virtual void WriteAccessorsStartTo(TranslationWriter writer)
            => writer.WriteToTranslation(" { ");

        /// <summary>
        /// Write characters to the given <paramref name="writer"/> to end translation of the
        /// property accessors.
        /// </summary>
        /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the characters.</param>
        protected virtual void WriteAccessorsEndTo(TranslationWriter writer)
            => writer.WriteToTranslation('}');

        /// <summary>
        /// An <see cref="ITranslatable"/> for a property accessor.
        /// </summary>
        protected class PropertyAccessorDefinitionTranslation : ITranslatable
        {
            private readonly string _accessor;

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyAccessorDefinitionTranslation"/>
            /// class.
            /// </summary>
            /// <param name="parent">
            /// The <see cref="PropertyDefinitionTranslation"/> to which the <paramref name="accessor"/>
            /// belongs.
            /// </param>
            /// <param name="accessor">
            /// An <see cref="IComplexMember"/> representing the accessor for which to create the
            /// <see cref="ITranslatable"/>.
            /// </param>
            /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
            public PropertyAccessorDefinitionTranslation(
                PropertyDefinitionTranslation parent,
                IMember accessor,
                TranslationSettings settings)
            {
                _accessor = IsGetter(accessor) ? "get" : "set";

                var translationSize = _accessor.Length + "; ".Length;
                FormattingSize = settings.GetKeywordFormattingSize();

                var accessibility = accessor.GetAccessibilityForTranslation();

                if (accessibility == parent._accessibility)
                {
                    TranslationSize = translationSize;
                    return;
                }

                _accessor = accessibility + _accessor;
                TranslationSize = translationSize + accessibility.Length;
            }

            #region Setup

            /// <summary>
            /// Gets a value indicating whether the given <paramref name="accessor"/> represents the
            /// property getter. If false, the accessor represents the property setter.
            /// </summary>
            /// <param name="accessor">
            ///  An <see cref="IMember"/> representing the accessor for which to make the determination.
            /// </param>
            /// <returns></returns>
            protected static bool IsGetter(IMember accessor)
                => accessor.Type != typeof(void);

            #endregion

            /// <inheritdoc />
            public virtual int TranslationSize { get; }

            /// <inheritdoc />
            public virtual int FormattingSize { get; }

            /// <inheritdoc />
            public int GetIndentSize() => 0;

            /// <inheritdoc />
            public int GetLineCount() => 1;

            /// <inheritdoc />
            public virtual void WriteTo(TranslationWriter writer)
            {
                WriteAccessorTo(writer);
                writer.WriteToTranslation("; ");
            }

            /// <summary>
            /// Writes the get or set accessor keyword to the given <paramref name="writer"/>, along
            /// with the appropriate modifier keywords ('internal', 'private', 'abstract', etc).
            /// </summary>
            /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the accessor.</param>
            protected void WriteAccessorTo(TranslationWriter writer)
                => writer.WriteKeywordToTranslation(_accessor);
        }
    }
}