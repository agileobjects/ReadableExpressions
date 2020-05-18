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
        private readonly int _boundTranslationCount;

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
                _boundTranslationCount = newArray.Expressions.Count;
                _boundTranslations = new ITranslation[_boundTranslationCount];

                for (var i = 0; i < _boundTranslationCount; ++i)
                {
                    var boundTranslation = context.GetTranslationFor(newArray.Expressions[i]);

                    _boundTranslations[i] = boundTranslation;
                    translationSize += boundTranslation.TranslationSize + 2;
                    formattingSize += boundTranslation.FormattingSize;
                }
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        public ExpressionType NodeType => ExpressionType.NewArrayBounds;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize()
        {
            var indentSize = _typeNameTranslation.GetIndentSize();

            switch (_boundTranslationCount)
            {
                case 0:
                    return indentSize;

                case 1:
                    return indentSize + _boundTranslations[0].GetIndentSize();

                default:
                    for (var i = 0; ;)
                    {
                        indentSize += _boundTranslations[i].GetIndentSize();

                        ++i;

                        if (i == _boundTranslationCount)
                        {
                            return indentSize;
                        }
                    }
            }
        }

        public int GetLineCount()
        {
            var lineCount = _typeNameTranslation.GetLineCount();

            if (_boundTranslationCount == 0)
            {
                return lineCount;
            }

            for (var i = 0; ;)
            {
                var boundLineCount = _boundTranslations[i].GetLineCount();

                if (boundLineCount > 1)
                {
                    lineCount += boundLineCount - 1;
                }

                ++i;

                if (i == _boundTranslationCount)
                {
                    return lineCount;
                }
            }
        }

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteNewToTranslation();
            _typeNameTranslation.WriteTo(buffer);
            buffer.WriteToTranslation('[');

            if (_boundTranslationCount != 0)
            {
                for (var i = 0; ;)
                {
                    _boundTranslations[i].WriteTo(buffer);

                    ++i;

                    if (i == _boundTranslationCount)
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