﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class BlockTranslation : ITranslation, IPotentialMultiStatementTranslatable
    {
        private readonly IDictionary<ITranslation, ParameterSetTranslation> _variables;
        private readonly IList<BlockStatementTranslation> _statements;
        private readonly bool _requiresReturnKeyword;
        private readonly bool _addBlankLineBeforeReturn;

        public BlockTranslation(BlockExpression block, ITranslationContext context)
        {
            _variables = GetVariableDeclarations(block, context);
            _statements = GetBlockStatements(block, context);
            EstimatedSize = GetEstimatedSize();
            IsMultiStatement = _statements.Count > 1;
            _requiresReturnKeyword = IsMultiStatement && block.IsReturnable();
            _addBlankLineBeforeReturn = _requiresReturnKeyword && AddBlankLineBeforeReturn(block);
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

        private static IList<BlockStatementTranslation> GetBlockStatements(BlockExpression block, ITranslationContext context)
        {
            var expressions = block.Expressions;
            var expressionCount = expressions.Count;
            var translations = new BlockStatementTranslation[expressionCount];
            var statementIndex = 0;

            for (int i = 0, lastExpressionIndex = expressionCount - 1; ; ++i)
            {
                var expression = expressions[i];

                if (Include(expression, block))
                {
                    translations[statementIndex++] = context.IsNotJoinedAssignment(expression)
                        ? new BlockStatementTranslation(expression, context)
                        : new BlockAssignmentStatementTranslation((BinaryExpression)expression, context);
                }

                if (i == lastExpressionIndex)
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

            if (expression.NodeType == ExpressionType.Parameter)
            {
                return false;
            }

            return (expression.NodeType != ExpressionType.Constant) || expression.IsComment();
        }

        private int GetEstimatedSize()
        {
            var estimatedSize = 0;

            if (_variables.Count != 0)
            {
                foreach (var parametersByType in _variables)
                {
                    estimatedSize += parametersByType.Key.EstimatedSize;
                    estimatedSize += parametersByType.Value.EstimatedSize;
                }
            }

            for (int i = 0, l = _statements.Count; i < l; i++)
            {
                estimatedSize += _statements[i].EstimatedSize;
            }

            return estimatedSize;
        }

        private static bool AddBlankLineBeforeReturn(BlockExpression block)
            => !block.Expressions[block.Expressions.Count - 2].IsComment();

        public ExpressionType NodeType => ExpressionType.Block;

        public int EstimatedSize { get; }

        public bool IsMultiStatement { get; }

        public BlockTranslation WithoutTermination()
        {
            _statements[_statements.Count - 1].DoNotTerminate = true;
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

            for (int i = 0, l = _statements.Count - 1; ; ++i)
            {
                var isFinalLine = i == l;

                if (isFinalLine && _requiresReturnKeyword)
                {
                    if (_addBlankLineBeforeReturn)
                    {
                        context.WriteNewLineToTranslation();
                    }

                    context.WriteToTranslation("return ");
                }

                _statements[i].WriteTo(context);

                if (isFinalLine)
                {
                    break;
                }

                context.WriteNewLineToTranslation();
            }
        }

        private class BlockStatementTranslation : ITranslation
        {
            private readonly ITranslation _statementTranslation;
            private readonly bool _statementIsUnterminated;

            public BlockStatementTranslation(Expression expression, ITranslationContext context)
            {
                NodeType = expression.NodeType;
                _statementTranslation = context.GetTranslationFor(expression);
                _statementIsUnterminated = StatementIsUnterminated(expression);
                EstimatedSize = _statementTranslation.EstimatedSize + 1;
            }

            private static bool StatementIsUnterminated(Expression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Block:
                    case ExpressionType.Lambda:
                        return false;

                    case ExpressionType.Assign:
                    case ExpressionType.MemberInit:
                        return true;
                }

                return /*translation.IsTerminated() || */!expression.IsComment();
            }

            public ExpressionType NodeType { get; }

            public int EstimatedSize { get; protected set; }

            public bool DoNotTerminate { private get; set; }

            public virtual void WriteTo(ITranslationContext context)
            {
                _statementTranslation.WriteTo(context);

                if (_statementIsUnterminated && (DoNotTerminate == false))
                {
                    context.WriteToTranslation(';');
                }
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
                    (assignment.Right.NodeType == ExpressionType.Lambda))
                {
                    return true;
                }

                if (assignment.Right.NodeType != ExpressionType.Conditional)
                {
                    return false;
                }

                var conditional = (ConditionalExpression)assignment.Right;

                return conditional.IfTrue.Type != conditional.IfFalse.Type;
            }

            public override void WriteTo(ITranslationContext context)
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

                base.WriteTo(context);
            }
        }
    }
}