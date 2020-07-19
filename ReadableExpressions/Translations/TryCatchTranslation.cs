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
    using static Formatting.TokenType;

    internal class TryCatchTranslation :
        ITranslation,
        IPotentialMultiStatementTranslatable,
        IPotentialSelfTerminatingTranslatable
    {
        private readonly bool _isNonVoidTryCatch;
        private readonly ITranslatable _bodyTranslation;
        private readonly IList<ITranslatable> _catchBlockTranslations;
        private readonly int _catchBlockCount;
        private readonly bool _hasFault;
        private readonly ITranslatable _faultTranslation;
        private readonly bool _hasFinally;
        private readonly ITranslatable _finallyTranslation;

        public TryCatchTranslation(TryExpression tryCatchFinally, ITranslationContext context)
        {
            Type = tryCatchFinally.Type;
            _isNonVoidTryCatch = tryCatchFinally.HasReturnType();

            _bodyTranslation = GetReturnableBlockTranslation(tryCatchFinally.Body, context);

            _catchBlockTranslations = GetCatchBlockTranslations(
                tryCatchFinally.Handlers,
                out _catchBlockCount,
                out var catchBlockTranslationsSize,
                out var catchBlocksFormattingSize,
                context);

            var translationSize = _bodyTranslation.TranslationSize + catchBlockTranslationsSize;
            var keywordFormattingSize = context.GetKeywordFormattingSize();
            
            var formattingSize = 
                keywordFormattingSize + // <- for the 'try'
               _bodyTranslation.FormattingSize + 
                catchBlocksFormattingSize;

            _hasFault = tryCatchFinally.Fault != null;

            if (_hasFault)
            {
                _faultTranslation = GetReturnableBlockTranslation(tryCatchFinally.Fault, context);
                translationSize += _faultTranslation.TranslationSize;
                formattingSize += keywordFormattingSize;
            }

            _hasFinally = tryCatchFinally.Finally != null;

            if (_hasFinally)
            {
                _finallyTranslation = GetReturnableBlockTranslation(tryCatchFinally.Finally, context);
                translationSize += _finallyTranslation.TranslationSize;
                formattingSize += keywordFormattingSize;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
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
            out int catchBlockCount,
            out int catchBlockTranslationsSize,
            out int catchBlocksFormattingSize,
            ITranslationContext context)
        {
            catchBlockCount = catchBlocks.Count;

            if (catchBlockCount == 0)
            {
                catchBlockTranslationsSize = catchBlocksFormattingSize = 0;
                return Enumerable<ITranslatable>.EmptyArray;
            }

            var catchBlockTranslations = new ITranslatable[catchBlockCount];

            catchBlockTranslationsSize = catchBlocksFormattingSize = 0;

            for (var i = 0; ;)
            {
                var catchBlockTranslation = new CatchBlockTranslation(catchBlocks[i], context);

                catchBlockTranslationsSize += catchBlockTranslation.TranslationSize;
                catchBlocksFormattingSize += catchBlockTranslation.FormattingSize;
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

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsMultiStatement => true;

        public bool IsTerminated => true;

        public int GetIndentSize()
        {
            var indentSize = _bodyTranslation.GetIndentSize();

            switch (_catchBlockCount)
            {
                case 0:
                    break;

                case 1:
                    indentSize += _catchBlockTranslations[0].GetIndentSize();
                    break;

                default:
                    for (var i = 0; ;)
                    {
                        indentSize += _catchBlockTranslations[i].GetIndentSize();

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
                indentSize += _faultTranslation.GetIndentSize();
            }

            if (_hasFinally)
            {
                indentSize += _finallyTranslation.GetIndentSize();
            }

            return indentSize;
        }

        public int GetLineCount()
        {
            var lineCount = _bodyTranslation.GetLineCount() + 1;

            switch (_catchBlockCount)
            {
                case 0:
                    break;

                case 1:
                    lineCount += _catchBlockTranslations[0].GetLineCount() + 1;
                    break;

                default:
                    for (var i = 0; ;)
                    {
                        lineCount += _catchBlockTranslations[i].GetLineCount() + 1;

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
                lineCount += _faultTranslation.GetLineCount() + 2;
            }

            if (_hasFinally)
            {
                lineCount += _finallyTranslation.GetLineCount() + 2;
            }

            return lineCount;
        }

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

                var keywordFormattingSize = context.GetKeywordFormattingSize();

                TranslationSize = _catchBodyTranslation.TranslationSize;
                FormattingSize = keywordFormattingSize + _catchBodyTranslation.FormattingSize;

                if (_exceptionClause != null)
                {
                    TranslationSize += _exceptionClause.TranslationSize;
                    FormattingSize += keywordFormattingSize + _exceptionClause.FormattingSize;
                }
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize() => _catchBodyTranslation.GetIndentSize();

            public int GetLineCount() => _catchBodyTranslation.GetLineCount() + 1;

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteNewLineToTranslation();
                writer.WriteKeywordToTranslation("catch");
                _exceptionClause?.WriteTo(writer);
                _catchBodyTranslation.WriteTo(writer);
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
                    TranslationSize = _exceptionTypeTranslation.TranslationSize + _variableName.Length + 5;
                    FormattingSize = _exceptionTypeTranslation.FormattingSize;
                }

                public virtual int TranslationSize { get; }

                public virtual int FormattingSize { get; }

                public int GetIndentSize() => _exceptionTypeTranslation.GetIndentSize();

                public int GetLineCount() => _exceptionTypeTranslation.GetLineCount();

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
                private readonly ITranslation _filterTranslation;

                public FilteredExceptionClause(CatchBlock catchBlock, ITranslationContext context)
                    : base(catchBlock, context)
                {
                    _filterTranslation = context.GetTranslationFor(catchBlock.Filter);
                    TranslationSize = base.TranslationSize + _filterTranslation.TranslationSize + 6;
                    FormattingSize = base.FormattingSize + _filterTranslation.FormattingSize;
                }

                public override int TranslationSize { get; }

                public override int FormattingSize { get; }

                public override void WriteTo(TranslationWriter writer)
                {
                    base.WriteTo(writer);
                    writer.WriteKeywordToTranslation(" when ");
                    _filterTranslation.WriteTo(writer);
                }
            }

            private class AnonymousExceptionClause : ITranslatable
            {
                private readonly ITranslation _exceptionTypeTranslation;

                public AnonymousExceptionClause(CatchBlock catchBlock, ITranslationContext context)
                {
                    _exceptionTypeTranslation = context.GetTranslationFor(catchBlock.Test);
                    TranslationSize = _exceptionTypeTranslation.TranslationSize + 3;
                }

                public int TranslationSize { get; }

                public int FormattingSize => _exceptionTypeTranslation.FormattingSize;

                public int GetIndentSize() => _exceptionTypeTranslation.GetIndentSize();

                public int GetLineCount() => _exceptionTypeTranslation.GetLineCount();

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