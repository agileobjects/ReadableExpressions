namespace AgileObjects.ReadableExpressions.Translations;

using System;
using System.Collections.Generic;
using Extensions;
using static Formatting.TokenType;

internal class TryCatchTranslation :
    INodeTranslation,
    IPotentialMultiStatementTranslatable,
    IPotentialSelfTerminatingTranslation
{
    private readonly bool _isNonVoidTryCatch;
    private readonly ITranslation _bodyTranslation;
    private readonly IList<ITranslation> _catchBlockTranslations;
    private readonly int _catchBlockCount;
    private readonly bool _hasFault;
    private readonly ITranslation _faultTranslation;
    private readonly bool _hasFinally;
    private readonly ITranslation _finallyTranslation;

    public TryCatchTranslation(
        TryExpression tryCatchFinally,
        ITranslationContext context)
    {
        _isNonVoidTryCatch = tryCatchFinally.HasReturnType();

        _bodyTranslation =
            GetReturnableBlockTranslation(tryCatchFinally.Body, context);

        _catchBlockTranslations = GetCatchBlockTranslations(
            tryCatchFinally.Handlers,
            out _catchBlockCount,
            out var catchBlockTranslationsSize,
            context);

        var translationLength = _bodyTranslation.TranslationLength + catchBlockTranslationsSize;

        _hasFault = tryCatchFinally.Fault != null;

        if (_hasFault)
        {
            _faultTranslation = GetReturnableBlockTranslation(tryCatchFinally.Fault, context);
            translationLength += _faultTranslation.TranslationLength;
        }

        _hasFinally = tryCatchFinally.Finally != null;

        if (_hasFinally)
        {
            _finallyTranslation =
                GetReturnableBlockTranslation(tryCatchFinally.Finally, context);

            translationLength += _finallyTranslation.TranslationLength;
        }

        TranslationLength = translationLength;
    }

    private INodeTranslation GetReturnableBlockTranslation(
        Expression block,
        ITranslationContext context)
    {
        var translation = GetBlockTranslation(block, context);

        if (_isNonVoidTryCatch)
        {
            translation = translation.WithReturnKeyword();
        }

        return translation;
    }

    private static CodeBlockTranslation GetBlockTranslation(
        Expression block,
        ITranslationContext context)
    {
        return context
            .GetCodeBlockTranslationFor(block)
            .WithTermination()
            .WithBraces();
    }

    private static IList<ITranslation> GetCatchBlockTranslations(
        IList<CatchBlock> catchBlocks,
        out int catchBlockCount,
        out int catchBlockTranslationsSize,
        ITranslationContext context)
    {
        catchBlockCount = catchBlocks.Count;

        if (catchBlockCount == 0)
        {
            catchBlockTranslationsSize = 0;
            return Enumerable<ITranslation>.EmptyArray;
        }

        var catchBlockTranslations = new ITranslation[catchBlockCount];

        catchBlockTranslationsSize = 0;

        for (var i = 0; ;)
        {
            var catchBlockTranslation =
                new CatchBlockTranslation(catchBlocks[i], context);

            catchBlockTranslationsSize += catchBlockTranslation.TranslationLength;
            catchBlockTranslations[i] = catchBlockTranslation;

            ++i;

            if (i == catchBlockCount)
            {
                break;
            }
        }

        return catchBlockTranslations;
    }

    public ExpressionType NodeType => ExpressionType.Try;

    public int TranslationLength { get; }

    public bool IsMultiStatement => true;

    public bool IsTerminated => true;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteKeywordToTranslation("try");
        _bodyTranslation.WriteTo(writer);

        switch (_catchBlockCount)
        {
            case 0:
                break;

            case 1:
                _catchBlockTranslations[0].WriteTo(writer);
                break;

            default:
                for (var i = 0; ;)
                {
                    _catchBlockTranslations[i].WriteTo(writer);

                    ++i;

                    if (i == _catchBlockCount)
                    {
                        break;
                    }
                }

                break;
        }

        if (_hasFault)
        {
            writer.WriteNewLineToTranslation();
            writer.WriteKeywordToTranslation("fault");
            _faultTranslation.WriteTo(writer);
        }

        if (_hasFinally)
        {
            writer.WriteNewLineToTranslation();
            writer.WriteKeywordToTranslation("finally");
            _finallyTranslation.WriteTo(writer);
        }
    }

    private class CatchBlockTranslation : ITranslation
    {
        private readonly CodeBlockTranslation _catchBodyTranslation;
        private readonly ITranslation _exceptionClause;

        public CatchBlockTranslation(
            CatchBlock catchBlock,
            ITranslationContext context)
        {
            _catchBodyTranslation = GetBlockTranslation(catchBlock.Body, context);
            _exceptionClause = GetExceptionClauseOrNullFor(catchBlock, context);

            if (_catchBodyTranslation.NodeType != ExpressionType.Throw &&
                catchBlock.Body.IsReturnable())
            {
                _catchBodyTranslation.WithReturnKeyword();
            }
        }

        public int TranslationLength =>
            _catchBodyTranslation.TranslationLength +
           (_exceptionClause?.TranslationLength ?? 0);

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteNewLineToTranslation();
            writer.WriteKeywordToTranslation("catch");
            _exceptionClause?.WriteTo(writer);
            _catchBodyTranslation.WriteTo(writer);
        }

        private static ITranslation GetExceptionClauseOrNullFor(
            CatchBlock catchBlock,
            ITranslationContext context)
        {
            if (ExceptionUsageFinder.IsVariableUsed(catchBlock))
            {
                return catchBlock.Filter != null
                    ? new FilteredExceptionClause(catchBlock, context)
                    : new NamedVariableExceptionClause(catchBlock, context);
            }

            return catchBlock.Test != typeof(Exception)
                ? new AnonymousExceptionClause(catchBlock, context) : null;
        }

        private class NamedVariableExceptionClause : ITranslation
        {
            private readonly INodeTranslation _exceptionTypeTranslation;
            private readonly string _variableName;

            public NamedVariableExceptionClause(
                CatchBlock catchBlock,
                ITranslationContext context)
            {
                _exceptionTypeTranslation = context.GetTranslationFor(catchBlock.Test);
                _variableName = catchBlock.Variable.Name;
            }

            public virtual int TranslationLength =>
                _exceptionTypeTranslation.TranslationLength +
                _variableName.Length + 5;

            public virtual void WriteTo(TranslationWriter writer)
            {
                writer.WriteToTranslation(" (");
                _exceptionTypeTranslation.WriteTo(writer);
                writer.WriteSpaceToTranslation();
                writer.WriteToTranslation(_variableName, Variable);
                writer.WriteToTranslation(')');
            }
        }

        private class FilteredExceptionClause : NamedVariableExceptionClause
        {
            private readonly INodeTranslation _filterTranslation;

            public FilteredExceptionClause(
                CatchBlock catchBlock,
                ITranslationContext context)
                : base(catchBlock, context)
            {
                _filterTranslation = context.GetTranslationFor(catchBlock.Filter);
            }

            public override int TranslationLength
                => base.TranslationLength + _filterTranslation.TranslationLength + 6;

            public override void WriteTo(TranslationWriter writer)
            {
                base.WriteTo(writer);
                writer.WriteKeywordToTranslation(" when ");
                _filterTranslation.WriteTo(writer);
            }
        }

        private class AnonymousExceptionClause : ITranslation
        {
            private readonly INodeTranslation _exceptionTypeTranslation;

            public AnonymousExceptionClause(
                CatchBlock catchBlock,
                ITranslationContext context)
            {
                _exceptionTypeTranslation = context.GetTranslationFor(catchBlock.Test);
            }

            public int TranslationLength
                => _exceptionTypeTranslation.TranslationLength + 3;

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteSpaceToTranslation();
                _exceptionTypeTranslation.WriteInParentheses(writer);
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
                    node.NodeType == ExpressionType.Throw &&
                    node.Operand == _catchHandler.Variable;

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