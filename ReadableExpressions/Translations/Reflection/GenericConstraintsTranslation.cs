namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Reflection;
    using Extensions;
    using static System.Reflection.GenericParameterAttributes;

    internal class GenericConstraintsTranslation : ITranslatable
    {
        private const string _where = "where ";
        private const string _class = "class";
        private const string _new = "new()";

        private readonly ITranslatable _parameterNameTranslation;
        private readonly bool _isClass;
        private readonly bool _isNewable;

        private GenericConstraintsTranslation(
            Type parameterType,
            GenericParameterAttributes constraints,
            TranslationSettings settings)
        {
            _parameterNameTranslation = new TypeNameTranslation(parameterType, settings);

            var keywordFormattingSize = settings.GetKeywordFormattingSize();

            var translationSize =
                _where.Length +
                _parameterNameTranslation.TranslationSize;

            var formattingSize =
                keywordFormattingSize +
                _parameterNameTranslation.FormattingSize;

            if ((constraints | ReferenceTypeConstraint) == constraints)
            {
                translationSize += _class.Length;
                formattingSize += keywordFormattingSize;
                _isClass = true;
            }

            if ((constraints | DefaultConstructorConstraint) == constraints)
            {
                translationSize += _new.Length;
                formattingSize += keywordFormattingSize;
                _isNewable = true;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        #region Factory Method

        public static ITranslatable For(Type genericParameterType, TranslationSettings settings)
        {
            var constraints = genericParameterType.GetConstraints();

            return constraints != None
                ? new GenericConstraintsTranslation(genericParameterType, constraints, settings)
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

            if (_isNewable)
            {
                if (constraintWritten)
                {
                    writer.WriteToTranslation(", ");
                }

                writer.WriteKeywordToTranslation("new");
                writer.WriteToTranslation("()");
            }
        }
    }
}