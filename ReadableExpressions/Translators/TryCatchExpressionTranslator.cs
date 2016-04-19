namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class TryCatchExpressionTranslator : ExpressionTranslatorBase
    {
        public TryCatchExpressionTranslator(Func<Expression, TranslationContext, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Try)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var tryCatchFinally = (TryExpression)expression;

            var tryBody = GetTranslatedExpressionBody(tryCatchFinally.Body, context);
            var catchBlocks = string.Join(string.Empty, tryCatchFinally.Handlers.Select(h => GetCatchBlock(h, context)));
            var faultBlock = GetFaultBlock(tryCatchFinally.Fault, context);
            var finallyBlock = GetFinallyBlock(tryCatchFinally.Finally, context);

            var tryCatchFinallyBlock = $@"
try{tryBody.WithParentheses()}
{catchBlocks}{faultBlock}{finallyBlock}";

            return tryCatchFinallyBlock.Trim();
        }

        private string GetCatchBlock(CatchBlock catchBlock, TranslationContext context)
        {
            var catchBody = GetTranslatedExpressionBody(catchBlock.Body, context);

            var exceptionClause = GetExceptionClause(catchBlock, context);

            var catchBodyBlock = catchBody
                .WithParentheses()
                .Replace($"throw {catchBlock.Variable.Name};", "throw;");

            return $@"catch{exceptionClause}{catchBodyBlock}
";
        }

        private string GetExceptionClause(CatchBlock catchBlock, TranslationContext context)
        {
            var exceptionTypeName = catchBlock.Variable.Type.GetFriendlyName();

            if (ExceptionUsageFinder.IsVariableUsed(catchBlock))
            {
                var filter = (catchBlock.Filter != null)
                    ? " when " + GetTranslation(catchBlock.Filter, context)
                    : null;

                return $" ({exceptionTypeName} {catchBlock.Variable.Name})" + filter;
            }

            if (catchBlock.Variable.Type != typeof(Exception))
            {
                return $" ({exceptionTypeName})";
            }

            return null;
        }

        private string GetFaultBlock(Expression faultBlock, TranslationContext context)
        {
            return GetHandlerBlock(faultBlock, "fault", context);
        }

        private string GetFinallyBlock(Expression finallyBlock, TranslationContext context)
        {
            return GetHandlerBlock(finallyBlock, "finally", context);
        }

        private string GetHandlerBlock(Expression block, string keyword, TranslationContext context)
        {
            if (block == null)
            {
                return null;
            }

            var blockBody = GetTranslatedExpressionBody(block, context).WithParentheses();

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
                _rethrowFound = node.NodeType == ExpressionType.Throw;

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