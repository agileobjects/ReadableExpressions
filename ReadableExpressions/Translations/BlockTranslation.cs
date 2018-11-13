namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Interfaces;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
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
            _variables = GetVariableDeclarations(block, context);
            _hasVariables = _variables.Count > 0;
            _statements = GetBlockStatements(block, context, out var hasMultiStatementStatement, out var estimatedStatementsSize, out _hasGoto);
            _statementCount = _statements.Count;
            EstimatedSize = GetEstimatedSize(estimatedStatementsSize);
            IsMultiStatement = hasMultiStatementStatement || (_statementCount > 1) || _hasVariables;
            IsTerminated = true;
        }

        private static IDictionary<ITranslation, ParameterSetTranslation> GetVariableDeclarations(
            BlockExpression block,
            ITranslationContext context)
        {
            var variablesByType = block
                .Variables
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
            out int estimatedStatementsSize,
            out bool hasGoto)
        {
            var expressions = block.Expressions;
            var expressionCount = expressions.Count;
            var translations = new BlockStatementTranslation[expressionCount];
            var statementIndex = 0;

            hasMultiStatementStatement = false;
            estimatedStatementsSize = 0;
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
                    estimatedStatementsSize += statementTranslation.EstimatedSize;
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
                    }
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

        private int GetEstimatedSize(int estimatedStatementsSize)
        {
            if (_variables.Count == 0)
            {
                return estimatedStatementsSize;
            }

            var estimatedSize = estimatedStatementsSize;

            foreach (var parametersByType in _variables)
            {
                estimatedSize += parametersByType.Key.EstimatedSize;
                estimatedSize += parametersByType.Value.EstimatedSize;
            }

            return estimatedSize;
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

        public int EstimatedSize { get; }

        public bool IsMultiStatement { get; }

        public bool IsTerminated { get; private set; }

        public bool HasGoto => _hasGoto;

        public BlockTranslation WithoutTermination()
        {
            _statements[_statementCount - 1].DoNotTerminate = true;
            IsTerminated = false;
            return this;
        }

        public void WriteTo(ITranslationContext context)
        {
            if (_hasVariables)
            {
                foreach (var parametersByType in _variables)
                {
                    parametersByType.Key.WriteTo(context);
                    context.WriteSpaceToTranslation();
                    parametersByType.Value.WriteTo(context);
                    context.WriteToTranslation(';');
                }

                context.WriteNewLineToTranslation();

                switch (_statements[0].NodeType)
                {
                    case Conditional when !ConditionalTranslation.IsTernary(_statements[0].Expression):
                    case Switch:
                        context.WriteNewLineToTranslation();
                        break;
                }
            }

            for (var i = 0; ;)
            {
                _statements[i].WriteTo(context);

                if (++i == _statementCount)
                {
                    break;
                }

                context.WriteNewLineToTranslation();
            }
        }

        private class BlockStatementTranslation : ITranslation, IPotentialMultiStatementTranslatable
        {
            private readonly ITranslation _statementTranslation;
            private readonly bool _statementIsUnterminated;
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
                EstimatedSize = _statementTranslation.EstimatedSize + 1;
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

                return !(expression.IsComment() || _statementTranslation.IsTerminated());
            }

            private bool WriteBlankLineBefore() => NodeType == Label;

            public ExpressionType NodeType { get; }

            public Expression Expression { get; }

            public int EstimatedSize { get; protected set; }

            public bool IsMultiStatement => _statementTranslation.IsMultiStatement();

            public bool DoNotTerminate { private get; set; }

            public void IsFirstStatement() => _writeBlankLineBefore = false;

            public void IsFinalStatement(bool leaveBlankLineBefore)
            {
                _writeBlankLineBefore = leaveBlankLineBefore;
                _suppressBlankLineAfter = true;
            }

            public void WriteReturnKeyword() => _writeReturnKeyword = true;

            public virtual bool HasGoto => _writeReturnKeyword || _statementTranslation.HasGoto();

            public void WriteTo(ITranslationContext context)
            {
                if (_writeBlankLineBefore)
                {
                    context.WriteNewLineToTranslation();
                }

                if (_writeReturnKeyword)
                {
                    context.WriteToTranslation("return ");
                }

                WriteStatementTo(context);

                if ((_suppressBlankLineAfter == false) && WriteBlankLineAfter())
                {
                    context.WriteNewLineToTranslation();
                }
            }

            protected virtual void WriteStatementTo(ITranslationContext context)
            {
                _statementTranslation.WriteTo(context);

                if (_statementIsUnterminated && (DoNotTerminate == false))
                {
                    context.WriteToTranslation(';');
                }
            }

            public bool WriteBlankLineAfter()
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
                if (UseFullTypeName(assignment))
                {
                    _typeNameTranslation = context.GetTranslationFor(assignment.Left.Type);
                    EstimatedSize += _typeNameTranslation.EstimatedSize + 2;
                    return;
                }

                EstimatedSize += _var.Length;
            }

            private static bool UseFullTypeName(BinaryExpression assignment)
            {
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

            protected override void WriteStatementTo(ITranslationContext context)
            {
                if (_typeNameTranslation != null)
                {
                    _typeNameTranslation.WriteTo(context);
                }
                else
                {
                    context.WriteToTranslation(_var);
                }

                context.WriteSpaceToTranslation();

                base.WriteStatementTo(context);
            }
        }
    }
}