namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class LambdaTranslation : ITranslation
    {
        private const string _fatArrow = " => ";

        private readonly ParameterSetTranslation _parameters;
        private readonly ITranslation _bodyTranslation;

        public LambdaTranslation(LambdaExpression lambda, ITranslationContext context)
        {
            _parameters = new ParameterSetTranslation(lambda.Parameters, context);
            _bodyTranslation = context.GetTranslationFor(lambda.Body);
            EstimatedSize = GetEstimatedSize();
        }

        private int GetEstimatedSize()
            => _parameters.EstimatedSize + _fatArrow.Length + _bodyTranslation.EstimatedSize;

        public ExpressionType NodeType => ExpressionType.Lambda;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _parameters.WriteTo(context);
            context.WriteToTranslation(_fatArrow);
            context.WriteCodeBlockToTranslation(_bodyTranslation);
        }
    }
}
