namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class TryCatchExpressionTranslator : ExpressionTranslatorBase
    {
        public TryCatchExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Try)
        {
        }

        public override string Translate(Expression expression)
        {
            var tryCatchFinally = (TryExpression)expression;

            var tryBody = GetTranslatedExpressionBody(tryCatchFinally.Body);
            var catchBlocks = string.Join(string.Empty, tryCatchFinally.Handlers.Select(GetCatchBlock));
            var faultBlock = GetFaultBlock(tryCatchFinally.Fault);
            var finallyBlock = GetFinallyBlock(tryCatchFinally.Finally);

            var tryCatchFinallyBlock = $@"
try{tryBody.WithBrackets()}
{catchBlocks}{faultBlock}{finallyBlock}";

            return tryCatchFinallyBlock.Trim();
        }

        private string GetCatchBlock(CatchBlock catchBlock)
        {
            var catchBody = GetTranslatedExpressionBody(catchBlock.Body);

            var exceptionClause = GetExceptionClause(catchBlock);

            var catchBodyBlock = catchBody
                .WithBrackets()
                .Replace($"throw {catchBlock.Variable.Name};", "throw;");

            return $@"catch{exceptionClause}{catchBodyBlock}
";
        }

        private string GetExceptionClause(CatchBlock catchBlock)
        {
            var exceptionTypeName = catchBlock.Variable.Type.GetFriendlyName();

            if (ExceptionUsageFinder.IsVariableUsed(catchBlock))
            {
                var filter = (catchBlock.Filter != null)
                    ? " when " + GetTranslation(catchBlock.Filter)
                    : null;

                return $" ({exceptionTypeName} {catchBlock.Variable.Name})" + filter;
            }

            if (catchBlock.Variable.Type != typeof(Exception))
            {
                return $" ({exceptionTypeName})";
            }

            return null;
        }

        private string GetFaultBlock(Expression faultBlock)
        {
            return GetHandlerBlock(faultBlock, "fault");
        }

        private string GetFinallyBlock(Expression finallyBlock)
        {
            return GetHandlerBlock(finallyBlock, "finally");
        }

        private string GetHandlerBlock(Expression block, string keyword)
        {
            if (block == null)
            {
                return null;
            }

            var blockBody = GetTranslatedExpressionBody(block).WithBrackets();

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