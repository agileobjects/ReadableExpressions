namespace AgileObjects.ReadableExpressions.Translations
{
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
            _typeNameTranslation = context.GetTranslationFor(newArray.Type.GetElementType());

            var estimatedSize = _typeNameTranslation.EstimatedSize + 6;
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

            EstimatedSize = estimatedSize;
        }

        public ExpressionType NodeType => ExpressionType.NewArrayBounds;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation("new ");
            _typeNameTranslation.WriteTo(context);
            context.WriteToTranslation('[');

            for (var i = 0; ;)
            {
                _boundTranslations[i].WriteTo(context);

                if (++i == _boundTranslations.Length)
                {
                    break;
                }

                context.WriteToTranslation("[]");
            }

            context.WriteToTranslation(']');
        }
    }
}