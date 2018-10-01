namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class LambdaTranslation : ITranslation
    {
        private readonly ParameterSetTranslation _parameters;
        private readonly ITranslation _body;

        public LambdaTranslation(LambdaExpression lambda, ITranslationContext context)
        {
            _parameters = new ParameterSetTranslation(lambda.Parameters);
            _body = context.GetTranslationFor(lambda.Body);
        }

        public int EstimatedSize { get; }

        public void Translate()
        {
            throw new System.NotImplementedException();
        }
    }
}
