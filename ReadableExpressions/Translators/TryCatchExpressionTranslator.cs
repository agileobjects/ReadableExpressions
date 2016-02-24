namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class TryCatchExpressionTranslator : ExpressionTranslatorBase
    {
        public TryCatchExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Try)
        {
        }

        public override string Translate(Expression expression)
        {
            var tryCatchFinally = (TryExpression)expression;

            var tryBody = Registry.TranslateExpressionBody(tryCatchFinally.Body);

            var catchBlocks = tryCatchFinally
                .Handlers
                .Select(catchHandler => GetCatchBlock(catchHandler));

            var catchBlocksCode = string.Join(Environment.NewLine, catchBlocks);

            var tryCatchFinallyBlock = $@"
try{tryBody.WithBrackets()}
{catchBlocksCode}";
            return tryCatchFinallyBlock.TrimStart();
        }

        private string GetCatchBlock(CatchBlock catchHandler)
        {
            var catchBody = Registry.TranslateExpressionBody(catchHandler.Body);

            var exceptionClause = catchHandler.Variable.Type != typeof(Exception)
                ? $" ({catchHandler.Variable.Type.GetFriendlyName()} {catchHandler.Variable.Name})"
                : null;

            return $"catch{exceptionClause}{catchBody.WithBrackets()}";
        }
    }
}