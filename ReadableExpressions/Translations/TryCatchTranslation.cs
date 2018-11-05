namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class TryCatchTranslation : ITranslation, IPotentialMultiStatementTranslatable
    {
        private readonly bool _isNonVoidTryCatch;
        private readonly ITranslatable _bodyTranslation;
        private readonly IList<ITranslatable> _catchBlockTranslations;
        private readonly bool _hasFault;
        private readonly ITranslatable _faultTranslation;
        private readonly bool _hasFinally;
        private readonly ITranslatable _finallyTranslation;

        public TryCatchTranslation(TryExpression tryCatchFinally, ITranslationContext context)
        {
            _isNonVoidTryCatch = tryCatchFinally.Type != typeof(void);

            _bodyTranslation = GetBlockTranslation(tryCatchFinally.Body, context);

            _catchBlockTranslations = GetCatchBlockTranslations(
                tryCatchFinally.Handlers,
                out var estimatedCatchBlocksSize,
                context);

            _hasFault = tryCatchFinally.Fault != null;

            if (_hasFault)
            {
                _faultTranslation = GetBlockTranslation(tryCatchFinally.Fault, context);
            }

            _hasFinally = tryCatchFinally.Finally != null;

            if (_hasFinally)
            {
                _finallyTranslation = GetBlockTranslation(tryCatchFinally.Finally, context);
            }

            EstimatedSize = GetEstimatedSize(estimatedCatchBlocksSize);
        }

        private ITranslatable GetBlockTranslation(Expression block, ITranslationContext context)
        {
            var translation = context
                .GetCodeBlockTranslationFor(block)
                .WithTermination()
                .WithBraces();

            if (_isNonVoidTryCatch)
            {
                translation = translation.WithReturnKeyword();
            }

            return translation;
        }

        private IList<ITranslatable> GetCatchBlockTranslations(
            IList<CatchBlock> catchBlocks,
            out int estimatedCatchBlocksSize,
            ITranslationContext context)
        {
            var catchBlockTranslations = new ITranslatable[catchBlocks.Count];

            estimatedCatchBlocksSize = 0;

            for (int i = 0, l = catchBlocks.Count; ;)
            {
                var catchBlockTranslation = new CatchBlockTranslation(catchBlocks[i], this, context);

                estimatedCatchBlocksSize += catchBlockTranslation.EstimatedSize;
                catchBlockTranslations[i] = catchBlockTranslation;

                if (++i == l)
                {
                    break;
                }
            }

            return catchBlockTranslations;
        }

        private int GetEstimatedSize(int estimatedCatchBlocksSize)
        {
            var estimatedSize = _bodyTranslation.EstimatedSize + estimatedCatchBlocksSize;

            if (_hasFault)
            {
                estimatedSize += _faultTranslation.EstimatedSize;
            }

            if (_hasFinally)
            {
                estimatedSize += _finallyTranslation.EstimatedSize;
            }

            return estimatedSize;
        }

        public ExpressionType NodeType => ExpressionType.Try;

        public int EstimatedSize { get; }

        public bool IsMultiStatement => true;

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation("try");
            _bodyTranslation.WriteTo(context);

            for (int i = 0, l = _catchBlockTranslations.Count; ;)
            {
                context.WriteNewLineToTranslation();
                _catchBlockTranslations[i].WriteTo(context);

                if (++i == l)
                {
                    break;
                }
            }

            if (_hasFault)
            {
                context.WriteToTranslation("fault");
                _faultTranslation.WriteTo(context);
            }

            if (_hasFinally)
            {
                context.WriteToTranslation("finally");
                _finallyTranslation.WriteTo(context);
            }
        }

        private class CatchBlockTranslation : ITranslatable
        {
            private readonly ITranslatable _catchBodyTranslation;
            private readonly ITranslatable _exceptionClause;

            public CatchBlockTranslation(
                CatchBlock catchBlock,
                TryCatchTranslation parent,
                ITranslationContext context)
            {
                _catchBodyTranslation = parent.GetBlockTranslation(catchBlock.Body, context);
                _exceptionClause = GetExceptionClauseOrNullFor(catchBlock, context);

                EstimatedSize = _catchBodyTranslation.EstimatedSize;

                if (_exceptionClause != null)
                {
                    EstimatedSize += _exceptionClause.EstimatedSize;
                }
            }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                context.WriteToTranslation("catch");
                _exceptionClause?.WriteTo(context);
                _catchBodyTranslation.WriteTo(context);
            }

            private static ITranslatable GetExceptionClauseOrNullFor(CatchBlock catchBlock, ITranslationContext context)
            {
                if (ExceptionUsageFinder.IsVariableUsed(catchBlock))
                {
                    return (catchBlock.Filter != null)
                        ? new FilteredExceptionClause(catchBlock, context)
                        : new NamedVariableExceptionClause(catchBlock, context);
                }

                return (catchBlock.Test != typeof(Exception))
                    ? new AnonymousExceptionClause(catchBlock, context) : null;
            }

            private class NamedVariableExceptionClause : ITranslatable
            {
                private readonly ITranslation _exceptionTypeTranslation;
                private readonly string _variableName;

                public NamedVariableExceptionClause(CatchBlock catchBlock, ITranslationContext context)
                {
                    _exceptionTypeTranslation = context.GetTranslationFor(catchBlock.Test);
                    _variableName = catchBlock.Variable.Name;
                    EstimatedSize = _exceptionTypeTranslation.EstimatedSize + _variableName.Length + 5;
                }

                public virtual int EstimatedSize { get; }

                public virtual void WriteTo(ITranslationContext context)
                {
                    context.WriteToTranslation(" (");
                    _exceptionTypeTranslation.WriteTo(context);
                    context.WriteSpaceToTranslation();
                    context.WriteToTranslation(_variableName);
                    context.WriteToTranslation(')');
                }
            }

            private class FilteredExceptionClause : NamedVariableExceptionClause
            {
                private readonly ITranslation _filterTranslation;

                public FilteredExceptionClause(CatchBlock catchBlock, ITranslationContext context)
                    : base(catchBlock, context)
                {
                    _filterTranslation = context.GetTranslationFor(catchBlock.Filter);
                    EstimatedSize = base.EstimatedSize + _filterTranslation.EstimatedSize + 6;
                }

                public override int EstimatedSize { get; }

                public override void WriteTo(ITranslationContext context)
                {
                    base.WriteTo(context);
                    context.WriteToTranslation(" when ");
                    _filterTranslation.WriteTo(context);
                }
            }

            private class AnonymousExceptionClause : ITranslatable
            {
                private readonly ITranslation _exceptionTypeTranslation;

                public AnonymousExceptionClause(CatchBlock catchBlock, ITranslationContext context)
                {
                    _exceptionTypeTranslation = context.GetTranslationFor(catchBlock.Test);
                    EstimatedSize = _exceptionTypeTranslation.EstimatedSize + 1;
                }

                public int EstimatedSize { get; }

                public void WriteTo(ITranslationContext context)
                {
                    context.WriteSpaceToTranslation();
                    _exceptionTypeTranslation.WriteInParentheses(context);
                }
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
}