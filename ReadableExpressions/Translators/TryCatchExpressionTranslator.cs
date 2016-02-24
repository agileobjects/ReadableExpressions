namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class TryCatchExpressionTranslator : ExpressionTranslatorBase
    {
        public TryCatchExpressionTranslator()
            : base(ExpressionType.Try)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var tryCatchFinally = (TryExpression)expression;

            var tryBody = translatorRegistry.TranslateExpressionBody(tryCatchFinally.Body);

            var catchBlocks = tryCatchFinally
                .Handlers
                .Select(catchHandler => GetCatchBlock(catchHandler, translatorRegistry));

            var catchBlocksCode = string.Join(Environment.NewLine, catchBlocks);

            var tryCatchFinallyBlock = $@"
try{tryBody.WithBrackets()}
{catchBlocksCode}";
            return tryCatchFinallyBlock.TrimStart();
        }

        private static string GetCatchBlock(CatchBlock catchHandler, IExpressionTranslatorRegistry translatorRegistry)
        {
            var catchBody = translatorRegistry.TranslateExpressionBody(catchHandler.Body);

            var exceptionClause = catchHandler.Variable.Type != typeof(Exception)
                ? $" ({catchHandler.Variable.Type.GetFriendlyName()} {catchHandler.Variable.Name})"
                : null;

            return $"catch{exceptionClause}{catchBody.WithBrackets()}";
        }
    }
}