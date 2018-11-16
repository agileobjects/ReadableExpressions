namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class LambdaTranslation : ITranslation
    {
        private const string _fatArrow = " => ";

        private readonly ParameterSetTranslation _parameters;
        private readonly CodeBlockTranslation _bodyTranslation;

        public LambdaTranslation(LambdaExpression lambda, ITranslationContext context)
        {
            Type = lambda.Type;
            _parameters = new ParameterSetTranslation(lambda.Parameters, context);
            _bodyTranslation = context.GetCodeBlockTranslationFor(lambda.Body);
            EstimatedSize = GetEstimatedSize();

            if (_bodyTranslation.IsMultiStatement == false)
            {
                _bodyTranslation.WithoutTermination();
            }
        }

        private int GetEstimatedSize()
            => _parameters.EstimatedSize + _fatArrow.Length + _bodyTranslation.EstimatedSize;

        public ExpressionType NodeType => ExpressionType.Lambda;

        public Type Type { get; }

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            _parameters.WriteTo(buffer);

            buffer.WriteToTranslation(_bodyTranslation.HasBraces ? " =>" : _fatArrow);

            _bodyTranslation.WriteTo(buffer);
        }
    }
}
