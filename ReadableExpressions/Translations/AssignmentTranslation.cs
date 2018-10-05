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
            _targetTranslation = context.GetTranslationFor(assignment.Left);
            _operator = _symbolsByNodeType[assignment.NodeType];
            _valueTranslation = context.GetTranslationFor(assignment.Right);

            EstimatedSize = GetEstimatedSize();
        }

        private int GetEstimatedSize()
        {
            return _targetTranslation.EstimatedSize +
                   _operator.Length +
                   _valueTranslation.EstimatedSize;
        }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _targetTranslation.WriteTo(context);
            context.WriteToTranslation(_operator);
            context.WriteToTranslation(' ');
            _valueTranslation.WriteTo(context);
        }
    }
}