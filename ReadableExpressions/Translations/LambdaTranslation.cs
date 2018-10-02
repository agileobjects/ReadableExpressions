namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class LambdaTranslation : ITranslation
    {
        private const string _fatArrow = "=> ";

        private readonly ParameterSetTranslation _parameters;
        private readonly ITranslation _body;
        private readonly Allocation _allocation;

        public LambdaTranslation(LambdaExpression lambda, ITranslationContext context)
        {
            _parameters = new ParameterSetTranslation(lambda.Parameters, context);
            _body = context.GetTranslationFor(lambda.Body);
            _allocation = context.Allocate(EstimatedSize = GetEstimatedSize());
        }

        private int GetEstimatedSize()
            => _parameters.EstimatedSize + _fatArrow.Length + _body.EstimatedSize;

        public int EstimatedSize { get; }

        public void Translate()
        {
            throw new System.NotImplementedException();
        }
    }
}
