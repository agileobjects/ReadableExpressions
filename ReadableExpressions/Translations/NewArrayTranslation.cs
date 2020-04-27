namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class NewArrayTranslation : ITranslation
    {
        private readonly ITranslation _typeNameTranslation;
        private readonly ITranslation[] _boundTranslations;

        public NewArrayTranslation(NewArrayExpression newArray, ITranslationContext context)
        {
            Type = newArray.Type;
            _typeNameTranslation = context.GetTranslationFor(newArray.Type.GetElementType());

            var estimatedSize = _typeNameTranslation.EstimatedSize + 6;

            if (newArray.Expressions.Count == 0)
            {
                _boundTranslations = Enumerable<ITranslation>.EmptyArray;
            }
            else
            {
                _boundTranslations = new ITranslation[newArray.Expressions.Count];

                for (var i = 0; ;)
                {
                    var boundTranslation = context.GetTranslationFor(newArray.Expressions[i]);

                    _boundTranslations[i] = boundTranslation;
                    estimatedSize += boundTranslation.EstimatedSize + 2;

                    if (++i == _boundTranslations.Length)
                    {
                        break;
                    }
                }
            }

            EstimatedSize = estimatedSize;
        }

        public ExpressionType NodeType => ExpressionType.NewArrayBounds;

        public Type Type { get; }

        public int EstimatedSize { get; }

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

                    if (++i == _boundTranslations.Length)
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