namespace AgileObjects.ReadableExpressions.Translations;

using Extensions;
using static ExpressionType;

internal class AssignmentTranslation :
    CheckedOperationTranslationBase,
    INodeTranslation,
    IPotentialSelfTerminatingTranslation
{
    private readonly INodeTranslation _targetTranslation;
    private readonly string _operator;
    private readonly INodeTranslation _valueTranslation;
    private bool _suppressSpaceBeforeValue;

    public AssignmentTranslation(
        BinaryExpression assignment,
        ITranslationContext context) :
        this(
            assignment.NodeType,
            GetTargetTranslation(assignment.Left, context),
            assignment.Right,
            context)
    {
    }

    private static INodeTranslation GetTargetTranslation(
        Expression target,
        ITranslationContext context)
    {
        return target.NodeType == Parameter
            ? context.GetTranslationFor(target)
            : context.GetCodeBlockTranslationFor(target);
    }

    public AssignmentTranslation(
        ExpressionType nodeType,
        INodeTranslation targetTranslation,
        Expression value,
        ITranslationContext context) :
        base(IsCheckedAssignment(nodeType), " { ", " }")
    {
        NodeType = nodeType;
        _targetTranslation = targetTranslation;
        _operator = GetOperatorOrNull(nodeType);
        _valueTranslation = GetValueTranslation(value, context);
    }

    private static bool IsCheckedAssignment(ExpressionType assignmentType)
    {
        return assignmentType switch
        {
            AddAssignChecked => true,
            MultiplyAssignChecked => true,
            SubtractAssignChecked => true,
            _ => false
        };
    }

    private static string GetOperatorOrNull(ExpressionType assignmentType)
    {
        return assignmentType switch
        {
            AddAssign => " +=",
            AddAssignChecked => " +=",
            AndAssign => " &=",
            Assign => " =",
            DivideAssign => " /=",
            ExclusiveOrAssign => " ^=",
            LeftShiftAssign => " <<=",
            ModuloAssign => " %=",
            MultiplyAssign => " *=",
            MultiplyAssignChecked => " *=",
            OrAssign => " |=",
            PowerAssign => " **=",
            RightShiftAssign => " >>=",
            SubtractAssign => " -=",
            SubtractAssignChecked => " -=",
            _ => null
        };
    }

    private INodeTranslation GetValueTranslation(
        Expression assignedValue,
        ITranslationContext context)
    {
        return assignedValue.NodeType == Default
            ? DefaultValueTranslation.For(assignedValue, context, allowNullKeyword: assignedValue.Type == typeof(string))
            : GetNonDefaultValueTranslation(assignedValue, context);
    }

    private INodeTranslation GetNonDefaultValueTranslation(
        Expression assignedValue,
        ITranslationContext context)
    {
        var valueBlock = context.GetCodeBlockTranslationFor(assignedValue);

        if (valueBlock.IsMultiStatement == false)
        {
            return IsCheckedOperation
                ? valueBlock.WithoutBraces().WithTermination()
                : valueBlock.WithoutBraces().WithoutTermination();
        }

        if (valueBlock.IsMultiStatementLambda)
        {
            return valueBlock.WithoutBraces();
        }

        _suppressSpaceBeforeValue = true;

        return assignedValue.NodeType == Conditional
            ? valueBlock.WithoutBraces()
            : valueBlock.WithBraces();
    }

    public static bool IsAssignment(ExpressionType nodeType) => 
        GetOperatorOrNull(nodeType) != null;

    public ExpressionType NodeType { get; }

    public int TranslationLength
    {
        get
        {
            var translationLength =
                _targetTranslation.TranslationLength +
                _operator.Length + 1 +
                _valueTranslation.TranslationLength;

            return IsCheckedOperation ? translationLength + 10 : translationLength;
        }
    }

    public bool IsTerminated => _valueTranslation.IsTerminated();

    protected override bool IsMultiStatement() => 
        _targetTranslation.IsMultiStatement() || _valueTranslation.IsMultiStatement();

    public void WriteTo(TranslationWriter writer)
    {
        WriteOpeningCheckedIfNecessary(writer, out var isMultiStatementChecked);
        _targetTranslation.WriteTo(writer);
        writer.WriteToTranslation(_operator);

        if (_suppressSpaceBeforeValue == false)
        {
            writer.WriteSpaceToTranslation();
        }

        _valueTranslation.WriteTo(writer);

        WriteClosingCheckedIfNecessary(writer, isMultiStatementChecked);
    }
}