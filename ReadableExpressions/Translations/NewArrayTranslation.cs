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

    internal class NewArrayTranslation : ITranslation
    {
        private readonly ITranslation _typeNameTranslation;
        private readonly ITranslation[] _boundTranslations;

        public NewArrayTranslation(NewArrayExpression newArray, ITranslationContext context)
        {
            Type = newArray.Type;
            _typeNameTranslation = context.GetTranslationFor(newArray.Type.GetElementType());

            var translationSize = _typeNameTranslation.TranslationSize + 6;
            var formattingSize = _typeNameTranslation.FormattingSize;

            if (newArray.Expressions.Count == 0)
            {
                _boundTranslations = Enumerable<ITranslation>.EmptyArray;
            }
            else
            {
                var translationsCount = newArray.Expressions.Count;
                _boundTranslations = new ITranslation[translationsCount];

                for (var i = 0; ;)
                {
                    var boundTranslation = context.GetTranslationFor(newArray.Expressions[i]);

                    _boundTranslations[i] = boundTranslation;
                    translationSize += boundTranslation.TranslationSize + 2;
                    formattingSize += boundTranslation.FormattingSize;

                    ++i;

                    if (i == translationsCount)
                    {
                        break;
                    }
                }
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        public ExpressionType NodeType => ExpressionType.NewArrayBounds;

        public Type Type { get; }

        public int TranslationSize { get; }
        
        public int FormattingSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteNewToTranslation();
            _typeNameTranslation.WriteTo(buffer);
            buffer.WriteToTranslation('[');

            if (_boundTranslations.Length != 0)
            {
                for (var i = 0; ;)
                {
                    _boundTranslations[i].WriteTo(buffer);

                    ++i;

                    if (i == _boundTranslations.Length)
                    {
                        break;
                    }

                    buffer.WriteToTranslation("[]");
                }
            }

            buffer.WriteToTranslation(']');
        }
    }
}