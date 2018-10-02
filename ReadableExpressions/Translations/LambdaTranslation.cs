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

        private readonly ITranslationContext _context;
        private readonly ParameterSetTranslation _parameters;
        private readonly ITranslation _body;

        public LambdaTranslation(LambdaExpression lambda, ITranslationContext context)
        {
            _context = context;
            _parameters = new ParameterSetTranslation(lambda.Parameters, context);
            _body = context.GetTranslationFor(lambda.Body);
            EstimatedSize = GetEstimatedSize();
        }

        private int GetEstimatedSize()
            => _parameters.EstimatedSize + _fatArrow.Length + _body.EstimatedSize;

        public int EstimatedSize { get; }

        public void WriteToTranslation()
        {
            _parameters.WriteToTranslation();
            _context.WriteToTranslation(_fatArrow);
            _body.WriteToTranslation();
        }
    }
}
