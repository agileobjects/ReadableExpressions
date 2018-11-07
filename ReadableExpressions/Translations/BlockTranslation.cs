namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
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
        private readonly IList<BlockStatementTranslation> _statements;
        private readonly int _statementCount;
        private readonly bool _hasGoto;

        public BlockTranslation(BlockExpression block, ITranslationContext context)
        {
            _variables = GetVariableDeclarations(block, context);
            _statements = GetBlockStatements(block, context, out var hasMultiStatementStatement, out var estimatedStatementsSize, out _hasGoto);
            _statementCount = _statements.Count;
            EstimatedSize = GetEstimatedSize(estimatedStatementsSize);
            IsMultiStatement = hasMultiStatementStatement || (_statementCount > 1) || (_variables.Count > 0);
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
                grp => context.GetTranslationFor(grp.Key),
                grp => new ParameterSetTranslation(grp, context).WithoutParentheses());
        }

        private static IList<BlockStatementTranslation> GetBlockStatements(
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

                if (Include(expression, block))
                {
                    var statementTranslation = context.IsNotJoinedAssignment(expression)
                        ? new BlockStatementTranslation(expression, context)
                        : new BlockAssignmentStatementTranslation((BinaryExpression)expression, context);

                    if (statementIndex == 0)
                    {
                        statementTranslation.IsFirstStatement();
                    }
                    else if (isFinalStatement && block.IsReturnable())
                    {
                        ConfigureFinalStatement(statementTranslation, translations, ref hasGoto);
                    }

                    translations[statementIndex++] = statementTranslation;
                    estimatedStatementsSize += statementTranslation.EstimatedSize;
                    hasMultiStatementStatement = hasMultiStatementStatement || statementTranslation.IsMultiStatement;
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

        private static bool Include(Expression expression, BlockExpression block)
        {
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

        private static void ConfigureFinalStatement(
            BlockStatementTranslation statementTranslation,
            IList<BlockStatementTranslation> statementTranslations,
            ref bool hasGoto)
        {
            if (statementTranslation.HasGoto)
            {
                hasGoto = true;
            }
            else
            {
                statementTranslation.WriteReturnKeyword();
            }

            statementTranslation.IsFinalStatement(AddBlankLineBeforeFinalStatement(statementTranslations));
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

        private static bool AddBlankLineBeforeFinalStatement(IList<BlockStatementTranslation> statementTranslations)
        {
            var penultimateTranslation = statementTranslations[statementTranslations.Count - 2];

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
            if (_variables.Count != 0)
            {
                foreach (var parametersByType in _variables)
                {
                    parametersByType.Key.WriteTo(context);
                    context.WriteSpaceToTranslation();
                    parametersByType.Value.WriteTo(context);
                    context.WriteToTranslation(';');
                }

                context.WriteNewLineToTranslation();
            }

            for (int i = 0, l = _statementCount - 1; ; ++i)
            {
                var statement = _statements[i];

                statement.WriteTo(context);

                if (i == l)
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

            public virtual bool HasGoto => _statementTranslation.HasGoto();

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