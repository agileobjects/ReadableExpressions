namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
    using NetStandardPolyfills;

    /// <summary>
    /// An <see cref="ITranslation"/> to translate a Type name.
    /// </summary>
    public class TypeNameTranslation : ITranslation
    {
        private const string _object = "object";
        private readonly TranslationSettings _settings;
        private readonly bool _isObject;
        private bool _writeObjectTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNameTranslation"/> class for the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type the name of which should be translated.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public TypeNameTranslation(Type type, TranslationSettings settings)
        {
            Type = type;
            _settings = settings;
            _isObject = type == typeof(object);

            var typeNameFormattingSize = settings.GetTypeNameFormattingSize();

            if (_isObject)
            {
                TranslationSize = _object.Length;
                FormattingSize = typeNameFormattingSize;
                return;
            }

            if (type.FullName == null)
            {
                return;
            }

            var translationSize = 0;
            var formattingSize = 0;

            if (type.IsGenericType())
            {
                translationSize += 2;
                formattingSize += 4; // <- for angle brackets

                foreach (var typeArgument in type.GetGenericTypeArguments())
                {
                    AddTypeNameSizes(
                        typeArgument,
                        typeNameFormattingSize,
                        ref translationSize,
                        ref formattingSize);
                }
            }

            AddTypeNameSizes(
                type,
                typeNameFormattingSize,
                ref translationSize,
                ref formattingSize);

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        private void AddTypeNameSizes(
            Type type,
            int typeNameFormattingSize,
            ref int translationSize,
            ref int formattingSize)
        {
            translationSize += type.GetSubstitutionOrNull()?.Length ?? type.Name.Length;
            formattingSize += typeNameFormattingSize;

            if (_settings.FullyQualifyTypeNames && (type.Namespace != null))
            {
                translationSize += type.Namespace.Length;
            }

            // ReSharper disable once PossibleNullReferenceException
            while (type.IsNested)
            {
                type = type.DeclaringType;

                AddTypeNameSizes(
                    type,
                    typeNameFormattingSize,
                    ref translationSize,
                    ref formattingSize);
            }
        }

        /// <inheritdoc />
        public ExpressionType NodeType => ExpressionType.Constant;

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        internal TypeNameTranslation WithObjectTypeName()
        {
            if (_isObject)
            {
                _writeObjectTypeName = true;
            }

            return this;
        }

        /// <inheritdoc />
        public int GetIndentSize() => 0;

        /// <inheritdoc />
        public int GetLineCount() => 1;

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            if (_isObject)
            {
                if (_writeObjectTypeName)
                {
                    writer.WriteTypeNameToTranslation("Object");
                    return;
                }

                writer.WriteKeywordToTranslation(_object);
                return;
            }

            writer.WriteFriendlyName(Type, _settings);
        }
    }
}