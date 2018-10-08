namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class BlockTranslation : ITranslation
    {
        private readonly IDictionary<ITranslation, ParameterSetTranslation> _variables;
        private readonly IList<ITranslation> _statements;

        public BlockTranslation(BlockExpression block, ITranslationContext context)
        {
            _variables = GetVariableDeclarations(block, context);
            _statements = GetBlockStatements(block.Expressions, context);
            EstimatedSize = GetEstimatedSize();
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

        private IList<ITranslation> GetBlockStatements(IList<Expression> expressions, ITranslationContext context)
        {
            var translations = new ITranslation[expressions.Count];

            for (int i = 0, l = expressions.Count; i < l; i++)
            {
                var expression = expressions[i];

                translations[i] = context.IsNotJoinedAssignment(expression)
                    ? new BlockStatementTranslation(expression, context)
                    : new BlockAssignmentStatementTranslation((BinaryExpression)expression, context);
            }

            return translations;
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

        public ExpressionType NodeType => ExpressionType.Block;

        public int EstimatedSize { get; }

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
                _statements[i].WriteTo(context);

                if (i == l)
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

            public virtual void WriteTo(ITranslationContext context)
            {
                _statementTranslation.WriteTo(context);

                if (_statementIsUnterminated)
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