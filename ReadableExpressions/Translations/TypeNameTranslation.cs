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

    internal class TypeNameTranslation : ITranslation
    {
        private const string _object = "object";
        private readonly TranslationSettings _settings;
        private readonly bool _isObject;
        private bool _writeObjectTypeName;

        public TypeNameTranslation(Type type, ITranslationContext context)
            : this(type, context.Settings)
        {
        }

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

        public ExpressionType NodeType => ExpressionType.Constant;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public TypeNameTranslation WithObjectTypeName()
        {
            if (_isObject)
            {
                _writeObjectTypeName = true;
            }

            return this;
        }

        public int GetIndentSize() => 0;

        public int GetLineCount() => 1;

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