namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
    using static TokenType;

    internal class TryCatchTranslation :
        ITranslation,
        IPotentialMultiStatementTranslatable,
        IPotentialSelfTerminatingTranslatable
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
            Type = tryCatchFinally.Type;
            _isNonVoidTryCatch = Type != typeof(void);

            _bodyTranslation = GetReturnableBlockTranslation(tryCatchFinally.Body, context);

            _catchBlockTranslations = GetCatchBlockTranslations(
                tryCatchFinally.Handlers,
                out var estimatedCatchBlocksSize,
                context);

            _hasFault = tryCatchFinally.Fault != null;

            if (_hasFault)
            {
                _faultTranslation = GetReturnableBlockTranslation(tryCatchFinally.Fault, context);
            }

            _hasFinally = tryCatchFinally.Finally != null;

            if (_hasFinally)
            {
                _finallyTranslation = GetReturnableBlockTranslation(tryCatchFinally.Finally, context);
            }

            EstimatedSize = GetEstimatedSize(estimatedCatchBlocksSize);
        }

        private ITranslation GetReturnableBlockTranslation(Expression block, ITranslationContext context)
        {
            var translation = GetBlockTranslation(block, context);

            if (_isNonVoidTryCatch)
            {
                translation = translation.WithReturnKeyword();
            }

            return translation;
        }

        private static CodeBlockTranslation GetBlockTranslation(Expression block, ITranslationContext context)
        {
            return context
                .GetCodeBlockTranslationFor(block)
                .WithTermination()
                .WithBraces();
        }

        private static IList<ITranslatable> GetCatchBlockTranslations(
            IList<CatchBlock> catchBlocks,
            out int estimatedCatchBlocksSize,
            ITranslationContext context)
        {
            if (catchBlocks.Count == 0)
            {
                estimatedCatchBlocksSize = 0;
                return Enumerable<ITranslatable>.EmptyArray;
            }

            var catchBlockTranslations = new ITranslatable[catchBlocks.Count];

            estimatedCatchBlocksSize = 0;

            for (int i = 0, l = catchBlocks.Count; ;)
            {
                var catchBlockTranslation = new CatchBlockTranslation(catchBlocks[i], context);

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
        
        public Type Type { get; }

        public int EstimatedSize { get; }

        public bool IsMultiStatement => true;

        public bool IsTerminated => true;

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteToTranslation("try", Keyword);
            _bodyTranslation.WriteTo(buffer);

            for (int i = 0, l = _catchBlockTranslations.Count; i < l; ++i)
            {
                buffer.WriteNewLineToTranslation();
                _catchBlockTranslations[i].WriteTo(buffer);
            }

            if (_hasFault)
            {
                buffer.WriteNewLineToTranslation();
                buffer.WriteToTranslation("fault", Keyword);
                _faultTranslation.WriteTo(buffer);
            }

            if (_hasFinally)
            {
                buffer.WriteNewLineToTranslation();
                buffer.WriteToTranslation("finally", Keyword);
                _finallyTranslation.WriteTo(buffer);
            }
        }

        private class CatchBlockTranslation : ITranslatable
        {
            private readonly CodeBlockTranslation _catchBodyTranslation;
            private readonly ITranslatable _exceptionClause;

            public CatchBlockTranslation(CatchBlock catchBlock, ITranslationContext context)
            {
                _catchBodyTranslation = GetBlockTranslation(catchBlock.Body, context);
                _exceptionClause = GetExceptionClauseOrNullFor(catchBlock, context);

                if ((_catchBodyTranslation.NodeType != ExpressionType.Throw) && catchBlock.Body.IsReturnable())
                {
                    _catchBodyTranslation.WithReturnKeyword();
                }

                EstimatedSize = _catchBodyTranslation.EstimatedSize;

                if (_exceptionClause != null)
                {
                    EstimatedSize += _exceptionClause.EstimatedSize;
                }
            }

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteToTranslation("catch", Keyword);
                _exceptionClause?.WriteTo(buffer);
                _catchBodyTranslation.WriteTo(buffer);
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

                public virtual void WriteTo(TranslationBuffer buffer)
                {
                    buffer.WriteToTranslation(" (");
                    _exceptionTypeTranslation.WriteTo(buffer);
                    buffer.WriteSpaceToTranslation();
                    buffer.WriteToTranslation(_variableName, Variable);
                    buffer.WriteToTranslation(')');
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

                public override void WriteTo(TranslationBuffer buffer)
                {
                    base.WriteTo(buffer);
                    buffer.WriteToTranslation(" when ", Keyword);
                    _filterTranslation.WriteTo(buffer);
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

                public void WriteTo(TranslationBuffer buffer)
                {
                    buffer.WriteSpaceToTranslation();
                    _exceptionTypeTranslation.WriteInParentheses(buffer);
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
                    if (_rethrowFound)
                    {
                        _rethrowFound = false;
                    }
                    else
                    {
                        if (node == _catchHandler.Variable)
                        {
                            _usageFound = true;
                        }
                    }

                    return base.VisitParameter(node);
                }
            }

            #endregion
        }
    }
}