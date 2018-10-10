namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class AssignmentTranslation : ITranslation
    {
        private static readonly Dictionary<ExpressionType, string> _symbolsByNodeType =
            new Dictionary<ExpressionType, string>
            {
                [AddAssign] = " +=",
                [AddAssignChecked] = " +=",
                [AndAssign] = " &=",
                [Assign] = " =",
                [DivideAssign] = " /=",
                [ExclusiveOrAssign] = " ^=",
                [LeftShiftAssign] = " <<=",
                [ModuloAssign] = " %=",
                [MultiplyAssign] = " *=",
                [MultiplyAssignChecked] = " *=",
                [OrAssign] = " |=",
                [PowerAssign] = " **=",
                [RightShiftAssign] = " >>=",
                [SubtractAssign] = " -=",
                [SubtractAssignChecked] = " -="
            };

        private readonly ITranslation _targetTranslation;
        private readonly string _operator;
        private readonly ITranslation _valueTranslation;

        public AssignmentTranslation(BinaryExpression assignment, ITranslationContext context)
        {
            NodeType = assignment.NodeType;
            _targetTranslation = context.GetTranslationFor(assignment.Left);
            _operator = _symbolsByNodeType[NodeType];
            _valueTranslation = GetValueTranslation(assignment.Right, context);
            EstimatedSize = GetEstimatedSize();
        }

        private static ITranslation GetValueTranslation(Expression assignedValue, ITranslationContext context)
        {
            return (assignedValue.NodeType == Default)
                ? new DefaultValueTranslation(assignedValue, context, allowNullKeyword: false)
                : context.GetTranslationFor(assignedValue);
        }

        private int GetEstimatedSize()
        {
            return _targetTranslation.EstimatedSize +
                   _operator.Length +
                   _valueTranslation.EstimatedSize;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _targetTranslation.WriteTo(context);
            context.WriteToTranslation(_operator);
            context.WriteSpaceToTranslation();
            context.WriteCodeBlockToTranslation(_valueTranslation);
        }
    }
}