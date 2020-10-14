namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using Extensions;

    internal class GenericConstraintsTranslation : ITranslatable
    {
        private const string _where = "where ";
        private const string _class = "class";
        private const string _struct = "struct";
        private const string _new = "new()";

        private readonly ITranslatable _parameterNameTranslation;
        private readonly bool _isClass;
        private readonly bool _isStruct;
        private readonly bool _isNewable;
        private readonly int _typeConstraintCount;
        private readonly ITranslatable[] _typeConstraintTranslations;

        private GenericConstraintsTranslation(
            IGenericArgument genericArgument,
            TranslationSettings settings)
        {
            _parameterNameTranslation = GenericArgumentTranslation
                .For(genericArgument, settings);

            var keywordFormattingSize = settings.GetKeywordFormattingSize();

            var translationSize =
                _where.Length +
                _parameterNameTranslation.TranslationSize;

            var formattingSize =
                keywordFormattingSize +
               _parameterNameTranslation.FormattingSize;

            if (genericArgument.HasClassConstraint)
            {
                translationSize += _class.Length;
                formattingSize += keywordFormattingSize + 2;
                _isClass = true;
            }

            if (genericArgument.HasStructConstraint)
            {
                translationSize += _struct.Length;
                formattingSize += keywordFormattingSize + 2;
                _isStruct = true;
            }

            if (genericArgument.HasNewableConstraint)
            {
                translationSize += _new.Length;
                formattingSize += keywordFormattingSize + 2;
                _isNewable = true;
            }

            var typeConstraints = genericArgument.TypeConstraints;
            _typeConstraintCount = typeConstraints.Count;

            if (_typeConstraintCount != 0)
            {
                translationSize += _typeConstraintCount * 2;
                _typeConstraintTranslations = new ITranslatable[_typeConstraintCount];

                for (var i = 0; i < _typeConstraintCount; ++i)
                {
                    var typeNameTranslation = new TypeNameTranslation(typeConstraints[i], settings);
                    translationSize += typeNameTranslation.TranslationSize;
                    formattingSize += typeNameTranslation.FormattingSize;
                    _typeConstraintTranslations[i] = typeNameTranslation;
                }
            }
            else
            {
                _typeConstraintTranslations = Enumerable<ITranslatable>.EmptyArray;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        #region Factory Method

        public static ITranslatable For(IGenericArgument genericArgument, TranslationSettings settings)
        {
            return genericArgument.HasConstraints
                ? new GenericConstraintsTranslation(genericArgument, settings)
                : NullTranslatable.Instance;
        }

        #endregion

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => 0;

        public int GetLineCount() => 1;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_where);
            _parameterNameTranslation.WriteTo(writer);
            writer.WriteToTranslation(" : ");

            var constraintWritten = false;

            if (_isClass)
            {
                writer.WriteKeywordToTranslation(_class);
                constraintWritten = true;
            }
            else if (_isStruct)
            {
                writer.WriteKeywordToTranslation(_struct);
                constraintWritten = true;
            }

            if (_typeConstraintCount != 0)
            {
                WriteSeparatorIfNecessary(constraintWritten, writer);

                for (var i = 0; ;)
                {
                    _typeConstraintTranslations[i].WriteTo(writer);

                    ++i;

                    if (i == _typeConstraintCount)
                    {
                        break;
                    }

                    writer.WriteToTranslation(", ");
                }

                constraintWritten = true;
            }

            if (_isNewable)
            {
                WriteSeparatorIfNecessary(constraintWritten, writer);
                writer.WriteKeywordToTranslation("new");
                writer.WriteToTranslation("()");
            }
        }

        private static void WriteSeparatorIfNecessary(
            bool constraintWritten,
            TranslationWriter writer)
        {
            if (constraintWritten)
            {
                writer.WriteToTranslation(", ");
            }
        }
    }
}