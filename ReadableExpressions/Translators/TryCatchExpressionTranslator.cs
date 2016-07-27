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

        public override string Translate(Expression expression, TranslationContext context)
        {
            var tryCatchFinally = (TryExpression)expression;

            var tryBody = context.TranslateCodeBlock(tryCatchFinally.Body);
            var catchBlocks = string.Join(string.Empty, tryCatchFinally.Handlers.Select(h => GetCatchBlock(h, context)));
            var faultBlock = GetFaultBlock(tryCatchFinally.Fault, context);
            var finallyBlock = GetFinallyBlock(tryCatchFinally.Finally, context);

            var tryCatchFinallyBlock = $@"
try{tryBody.WithCurlyBraces()}
{catchBlocks}{faultBlock}{finallyBlock}";

            return tryCatchFinallyBlock.Trim();
        }

        private static string GetCatchBlock(CatchBlock catchBlock, TranslationContext context)
        {
            var catchBody = context.TranslateCodeBlock(catchBlock.Body);

            var exceptionClause = GetExceptionClause(catchBlock, context);

            var catchBodyBlock = catchBody.WithCurlyBraces();

            if (catchBlock.Variable != null)
            {
                catchBodyBlock = catchBodyBlock
                    .Replace($"throw {catchBlock.Variable.Name};", "throw;");
            }

            return $@"catch{exceptionClause}{catchBodyBlock}
";
        }

        private static string GetExceptionClause(CatchBlock catchBlock, TranslationContext context)
        {
            var exceptionTypeName = catchBlock.Test.GetFriendlyName();

            if (ExceptionUsageFinder.IsVariableUsed(catchBlock))
            {
                var filter = (catchBlock.Filter != null)
                    ? " when " + context.Translate(catchBlock.Filter)
                    : null;

                return $" ({exceptionTypeName} {catchBlock.Variable.Name})" + filter;
            }

            return (catchBlock.Test != typeof(Exception))
                ? $" ({exceptionTypeName})"
                : null;
        }

        private static string GetFaultBlock(Expression faultBlock, TranslationContext context)
        {
            return GetHandlerBlock(faultBlock, "fault", context);
        }

        private static string GetFinallyBlock(Expression finallyBlock, TranslationContext context)
        {
            return GetHandlerBlock(finallyBlock, "finally", context);
        }

        private static string GetHandlerBlock(Expression block, string keyword, TranslationContext context)
        {
            if (block == null)
            {
                return null;
            }

            var blockBody = context.TranslateCodeBlock(block).WithCurlyBraces();

            return keyword + blockBody;
        }

        #region ExceptionUsageFinder

        private class ExceptionUsageFinder : ExpressionVisitor
        {
            private readonly CatchBlock _catchHandler;
            private bool _rethrowFound;
            private bool _usageFound;

            private ExceptionUsageFinder(CatchBlock catchHandler)
            {
                _catchHandler = catchHandler;
            }

            public static bool IsVariableUsed(CatchBlock catchHandler)
            {
                if (catchHandler.Variable == null)
                {
                    return false;
                }

                var visitor = new ExceptionUsageFinder(catchHandler);
                visitor.Visit(catchHandler.Filter);

                if (!visitor._usageFound)
                {
                    visitor.Visit(catchHandler.Body);
                }

                return visitor._usageFound;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                _rethrowFound =
                    (node.NodeType == ExpressionType.Throw) &&
                    (node.Operand == _catchHandler.Variable);

                return base.VisitUnary(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (!_rethrowFound)
                {
                    if (node == _catchHandler.Variable)
                    {
                        _usageFound = true;
                    }
                }
                else
                {
                    _rethrowFound = false;
                }

                return base.VisitParameter(node);
            }
        }

        #endregion
    }
}