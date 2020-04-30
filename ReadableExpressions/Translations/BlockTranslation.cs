﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class BlockTranslation :
        ITranslation,
        IPotentialMultiStatementTranslatable,
        IPotentialSelfTerminatingTranslatable,
        IPotentialGotoTranslatable
    {
        private readonly IDictionary<ITranslation, ParameterSetTranslation> _variables;
        private readonly bool _hasVariables;
        private readonly IList<BlockStatementTranslation> _statements;
        private readonly int _statementCount;
        private readonly bool _hasGoto;

        public BlockTranslation(BlockExpression block, ITranslationContext context)
        {
            Type = block.Type;
            _variables = GetVariableDeclarations(block, context);
            _hasVariables = _variables.Count > 0;

            _statements = GetBlockStatements(
                block,
                context,
                out var hasMultiStatementStatement,
                out var statementTranslationsSize,
                out var statementsFormattingSize,
                out _hasGoto);

            _statementCount = _statements.Count;
            IsMultiStatement = hasMultiStatementStatement || (_statementCount > 1) || _hasVariables;
            IsTerminated = true;

            if (!_hasVariables)
            {
                TranslationSize = statementTranslationsSize;
                return;
            }

            var translationSize = statementTranslationsSize;
            var formattingSize = statementsFormattingSize;

            foreach (var parametersByType in _variables)
            {
                translationSize +=
                    parametersByType.Key.TranslationSize +
                    parametersByType.Value.TranslationSize;

                formattingSize +=
                    parametersByType.Key.FormattingSize +
                    parametersByType.Value.FormattingSize;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        private static IDictionary<ITranslation, ParameterSetTranslation> GetVariableDeclarations(
            BlockExpression block,
            ITranslationContext context)
        {
            var variablesByType = block
                .Variables
                .Except(context.InlineOutputVariables)
                .Except(context.JoinedAssignmentVariables)
                .GroupBy(v => v.Type)
                .ToArray();

            if (variablesByType.Length == 0)
            {
                return EmptyDictionary<ITranslation, ParameterSetTranslation>.Instance;
            }

            return variablesByType.ToDictionary(
                grp => (ITranslation)context.GetTranslationFor(grp.Key),
                grp => new ParameterSetTranslation(grp, context).WithoutParentheses());
        }

        private IList<BlockStatementTranslation> GetBlockStatements(
            BlockExpression block,
            ITranslationContext context,
            out bool hasMultiStatementStatement,
            out int statementTranslationsSize,
            out int statementsFormattingSize,
            out bool hasGoto)
        {
            var expressions = block.Expressions;
            var expressionCount = expressions.Count;
            var translations = new BlockStatementTranslation[expressionCount];
            var statementIndex = 0;

            hasMultiStatementStatement = false;
            statementTranslationsSize = 0;
            statementsFormattingSize = 0;
            hasGoto = false;

            for (int i = 0, lastExpressionIndex = expressionCount - 1; ; ++i)
            {
                var isFinalStatement = i == lastExpressionIndex;
                var expression = expressions[i];

                if (Include(expression, block, context))
                {
                    var statementTranslation = context.IsNotJoinedAssignment(expression)
                        ? new BlockStatementTranslation(expression, context)
                        : new BlockAssignmentStatementTranslation((BinaryExpression)expression, context);

                    translations[statementIndex++] = statementTranslation;
                    statementTranslationsSize += statementTranslation.TranslationSize;
                    statementsFormattingSize += statementTranslation.FormattingSize;
                    hasMultiStatementStatement = hasMultiStatementStatement || statementTranslation.IsMultiStatement;

                    if (statementIndex == 1)
                    {
                        statementTranslation.IsFirstStatement();
                    }

                    if (isFinalStatement)
                    {
                        var isReturnable = block.IsReturnable();

                        if (isReturnable)
                        {
                            ConfigureFinalStatementReturn(statementTranslation, statementIndex, ref hasGoto);
                        }

                        var addBlankLineBefore =
                            isReturnable && AddBlankLineBeforeFinalStatement(statementIndex, translations);

                        statementTranslation.IsFinalStatement(addBlankLineBefore);
                        break;
                    }

                    continue;
                }

                if (isFinalStatement)
                {
                    break;
                }
            }

            if (statementIndex == expressionCount)
            {
                return translations;
            }

            // Statements were skipped; resize the translations array
            var includedTranslations = new BlockStatementTranslation[statementIndex];

            for (var i = 0; i < statementIndex; ++i)
            {
                includedTranslations[i] = translations[i];
            }

            return includedTranslations;
        }

        private static bool Include(Expression expression, BlockExpression block, ITranslationContext context)
        {
            switch (expression.NodeType)
            {
                case Label:
                    return (expression.Type != typeof(void)) ||
                           context.IsReferencedByGoto(((LabelExpression)expression).Target);

                case Default when expression.Type == typeof(void):
                    return false;
            }

            if (expression == block.Result)
            {
                return true;
            }

            if (expression.NodeType == Parameter)
            {
                return false;
            }

            return (expression.NodeType != Constant) || expression.IsComment();
        }

        private void ConfigureFinalStatementReturn(
            BlockStatementTranslation statementTranslation,
            int translationCount,
            ref bool hasGoto)
        {
            if (statementTranslation.HasGoto)
            {
                hasGoto = true;
            }
            else if (_hasVariables || translationCount > 1)
            {
                statementTranslation.WriteReturnKeyword();
                hasGoto = true;
            }
        }

        private static bool AddBlankLineBeforeFinalStatement(
            int translationCount,
            IList<BlockStatementTranslation> statementTranslations)
        {
            if (translationCount < 2)
            {
                return false;
            }

            var penultimateTranslation = statementTranslations[translationCount - 2];

            switch (penultimateTranslation.NodeType)
            {
                case Label:
                    return false;
            }

            if (penultimateTranslation.WriteBlankLineAfter())
            {
                return false;
            }

            return !penultimateTranslation.Expression.IsComment();
        }

        public ExpressionType NodeType => Block;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsMultiStatement { get; }

        public bool IsTerminated { get; private set; }

        public bool HasGoto => _hasGoto;

        public BlockTranslation WithoutTermination()
        {
            _statements[_statementCount - 1].DoNotTerminate = true;
            IsTerminated = false;
            return this;
        }

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_hasVariables)
            {
                foreach (var parametersByType in _variables)
                {
                    parametersByType.Key.WriteTo(buffer);
                    buffer.WriteSpaceToTranslation();
                    parametersByType.Value.WriteTo(buffer);
                    buffer.WriteToTranslation(';');
                    buffer.WriteNewLineToTranslation();
                }

                switch (_statements[0].NodeType)
                {
                    case Conditional when !ConditionalTranslation.IsTernary(_statements[0].Expression):
                    case Switch:
                        buffer.WriteNewLineToTranslation();
                        break;
                }
            }

            for (var i = 0; ;)
            {
                _statements[i].WriteTo(buffer);

                ++i;

                if (i == _statementCount)
                {
                    break;
                }

                buffer.WriteNewLineToTranslation();
            }
        }

        private class BlockStatementTranslation : ITranslation, IPotentialMultiStatementTranslatable
        {
            private readonly ITranslation _statementTranslation;
            private readonly bool _statementIsUnterminated;
            private bool? _isMultiStatement;
            private bool _writeBlankLineBefore;
            private bool _suppressBlankLineAfter;
            private bool _writeReturnKeyword;

            public BlockStatementTranslation(Expression expression, ITranslationContext context)
            {
                NodeType = expression.NodeType;
                Expression = expression;
                _statementTranslation = context.GetTranslationFor(expression);
                _statementIsUnterminated = StatementIsUnterminated(expression);
                _writeBlankLineBefore = WriteBlankLineBefore();
                TranslationSize = _statementTranslation.TranslationSize + 1;
                FormattingSize = _statementTranslation.FormattingSize;
            }

            private bool StatementIsUnterminated(Expression expression)
            {
                switch (NodeType)
                {
                    case Block:
                    case Lambda:
                        return false;

                    case MemberInit:
                        return true;
                }

                if (_statementTranslation.IsAssignment())
                {
                    return true;
                }

                return !(expression.IsComment() || _statementTranslation.IsTerminated());
            }

            private bool WriteBlankLineBefore()
            {
                return (NodeType == Label) ||
                      ((NodeType == Conditional) && !ConditionalTranslation.IsTernary(Expression));
            }

            public ExpressionType NodeType { get; }

            public Type Type => Expression.Type;

            public Expression Expression { get; }

            public int TranslationSize { get; protected set; }

            public int FormattingSize { get; protected set; }

            public bool IsMultiStatement
                => _isMultiStatement ?? (_isMultiStatement = _statementTranslation.IsMultiStatement()).Value;

            public bool DoNotTerminate { private get; set; }

            public void IsFirstStatement() => _writeBlankLineBefore = false;

            public void IsFinalStatement(bool leaveBlankLineBefore)
            {
                if ((_writeBlankLineBefore == false) && leaveBlankLineBefore)
                {
                    _writeBlankLineBefore = true;
                }

                _suppressBlankLineAfter = true;
            }

            public void WriteReturnKeyword() => _writeReturnKeyword = true;

            public virtual bool HasGoto => _writeReturnKeyword || _statementTranslation.HasGoto();

            public void WriteTo(TranslationBuffer buffer)
            {
                if ((_writeBlankLineBefore || buffer.TranslationQuery(q => q.TranslationEndsWith("};"))) &&
                    !buffer.TranslationQuery(q => q.TranslationEndsWithBlankLine()))
                {
                    buffer.WriteNewLineToTranslation();
                }

                if (_writeReturnKeyword)
                {
                    buffer.WriteReturnToTranslation();
                }

                WriteStatementTo(buffer);

                if ((_suppressBlankLineAfter == false) && WriteBlankLineAfter())
                {
                    buffer.WriteNewLineToTranslation();
                }
            }

            protected virtual void WriteStatementTo(TranslationBuffer buffer)
            {
                _statementTranslation.WriteTo(buffer);

                if (_statementIsUnterminated && (DoNotTerminate == false))
                {
                    buffer.WriteToTranslation(';');
                }
            }

            public virtual bool WriteBlankLineAfter()
            {
                switch (NodeType)
                {
                    case Conditional:
                    case Lambda:
                        return true;
                }

                return false;
            }
        }

        private class BlockAssignmentStatementTranslation : BlockStatementTranslation
        {
            private const string _var = "var";
            private readonly ITranslatable _typeNameTranslation;

            public BlockAssignmentStatementTranslation(BinaryExpression assignment, ITranslationContext context)
                : base(assignment, context)
            {
                if (UseFullTypeName(assignment, context))
                {
                    _typeNameTranslation = context.GetTranslationFor(assignment.Left.Type);
                    TranslationSize += _typeNameTranslation.TranslationSize + 2;
                    FormattingSize += _typeNameTranslation.FormattingSize;
                    return;
                }

                TranslationSize += _var.Length;
                FormattingSize += context.GetKeywordFormattingSize();
            }

            private static bool UseFullTypeName(BinaryExpression assignment, ITranslationContext context)
            {
                if (!context.Settings.UseImplicitTypeNames)
                {
                    return true;
                }

                if ((assignment.Left.Type != assignment.Right.Type) ||
                    (assignment.Right.NodeType == Lambda))
                {
                    return true;
                }

                if (assignment.Right.NodeType != Conditional)
                {
                    return false;
                }

                var conditional = (ConditionalExpression)assignment.Right;

                return conditional.IfTrue.Type != conditional.IfFalse.Type;
            }

            public override bool HasGoto => false;

            protected override void WriteStatementTo(TranslationBuffer buffer)
            {
                if (_typeNameTranslation != null)
                {
                    _typeNameTranslation.WriteTo(buffer);
                }
                else
                {
                    buffer.WriteKeywordToTranslation(_var);
                }

                buffer.WriteSpaceToTranslation();

                base.WriteStatementTo(buffer);
            }

            public override bool WriteBlankLineAfter() => IsMultiStatement;
        }
    }
}