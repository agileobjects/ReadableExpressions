namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
using static Microsoft.Scripting.Ast.ExpressionType;
#else
using System.Linq.Expressions;
using static System.Linq.Expressions.ExpressionType;
#endif

internal class BinaryTranslation :
    CheckedOperationTranslationBase,
    INodeTranslation,
    IPotentialParenthesizedTranslation
{
    private readonly ITranslationContext _context;
    private readonly INodeTranslation _leftOperandTranslation;
    private readonly string _operator;
    private readonly INodeTranslation _rightOperandTranslation;
    private bool _suppressParentheses;

    private BinaryTranslation(BinaryExpression binary, ITranslationContext context) :
        base(IsChecked(binary.NodeType), "(", ")")
    {
        _context = context;
        NodeType = binary.NodeType;
        _leftOperandTranslation = context.GetTranslationFor(binary.Left);
        _operator = GetOperator(NodeType);
        _rightOperandTranslation = context.GetTranslationFor(binary.Right);

        if (_leftOperandTranslation is BinaryTranslation leftNestedBinary &&
             HasComplimentaryOperator(leftNestedBinary))
        {
            leftNestedBinary._suppressParentheses = true;
        }

        if (_rightOperandTranslation is BinaryTranslation rightNestedBinary &&
             HasComplimentaryOperator(rightNestedBinary))
        {
            rightNestedBinary._suppressParentheses = true;
        }
    }

    #region Setup

    private static bool IsChecked(ExpressionType nodeType)
    {
        return nodeType switch
        {
            AddChecked => true,
            MultiplyChecked => true,
            SubtractChecked => true,
            _ => false
        };
    }

    private static string GetOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            Add => " + ",
            AddChecked => " + ",
            And => " & ",
            AndAlso => " && ",
            Coalesce => " ?? ",
            Divide => " / ",
            Equal => " == ",
            ExclusiveOr => " ^ ",
            GreaterThan => " > ",
            GreaterThanOrEqual => " >= ",
            LeftShift => " << ",
            LessThan => " < ",
            LessThanOrEqual => " <= ",
            Modulo => " % ",
            Multiply => " * ",
            MultiplyChecked => " * ",
            NotEqual => " != ",
            Or => " | ",
            OrElse => " || ",
            Power => " ** ",
            RightShift => " >> ",
            Subtract => " - ",
            SubtractChecked => " - ",
            _ => null
        };
    }

    private bool HasComplimentaryOperator(INodeTranslation otherBinary)
    {
        switch (otherBinary.NodeType)
        {
            case Add:
            case AddChecked:
            case Subtract:
            case SubtractChecked:
                return NodeType is Add or AddChecked or Subtract or SubtractChecked;

            case Multiply:
            case MultiplyChecked:
            case Divide:
                return NodeType is Multiply or MultiplyChecked or Divide;
        }

        return false;
    }

    #endregion

    #region Factory Methods

    public static INodeTranslation For(
        BinaryExpression binary,
        ITranslationContext context)
    {
        switch (binary.NodeType)
        {
            case Add:
                if (binary.Type != typeof(string))
                {
                    break;
                }

                return StringConcatenationTranslation.ForAddition(binary, context);

            case Equal:
            case NotEqual:
                if (StandaloneEqualityComparisonTranslation.TryGetTranslation(binary, context, out var translation))
                {
                    return translation;
                }

                break;
        }

        return new BinaryTranslation(binary, context);
    }

    #endregion

    public static bool IsBinary(ExpressionType nodeType)
        => GetOperator(nodeType) != null;

    public static string GetOperator(Expression expression)
        => GetOperator(expression.NodeType);

    public ExpressionType NodeType { get; }

    public int TranslationLength
    {
        get
        {
            var translationLength =
                _leftOperandTranslation.TranslationLength +
                _operator.Length +
                _rightOperandTranslation.TranslationLength;

            return IsCheckedOperation ? translationLength + 10 : translationLength;
        }
    }

    bool IPotentialParenthesizedTranslation.Parenthesize
        => !_suppressParentheses;

    public void WriteTo(TranslationWriter writer)
    {
        WriteOpeningCheckedIfNecessary(writer, out var isMultiStatementChecked);
        _leftOperandTranslation.WriteInParenthesesIfRequired(writer, _context);
        writer.WriteToTranslation(_operator);
        _rightOperandTranslation.WriteInParenthesesIfRequired(writer, _context);
        WriteClosingCheckedIfNecessary(writer, isMultiStatementChecked);
    }

    protected override bool IsMultiStatement()
        => _leftOperandTranslation.IsMultiStatement() || _rightOperandTranslation.IsMultiStatement();

    private class StandaloneEqualityComparisonTranslation : INodeTranslation
    {
        private readonly ITranslationContext _context;
        private readonly StandaloneBoolean _standaloneBoolean;
        private readonly INodeTranslation _operandTranslation;

        private StandaloneEqualityComparisonTranslation(
            ExpressionType nodeType,
            Expression boolean,
            ExpressionType @operator,
            Expression comparison,
            ITranslationContext context)
        {
            _context = context;
            NodeType = nodeType;
            _standaloneBoolean = new StandaloneBoolean(boolean, @operator, comparison);
            _operandTranslation = context.GetTranslationFor(_standaloneBoolean.Expression);
        }

        public static bool TryGetTranslation(
            BinaryExpression comparison,
            ITranslationContext context,
            out INodeTranslation translation)
        {
            if (IsBooleanConstant(comparison.Right))
            {
                translation = new StandaloneEqualityComparisonTranslation(
                    comparison.NodeType,
                    comparison.Left,
                    comparison.NodeType,
                    comparison.Right,
                    context);

                return true;
            }

            if (IsBooleanConstant(comparison.Left))
            {
                translation = new StandaloneEqualityComparisonTranslation(
                    comparison.NodeType,
                    comparison.Right,
                    comparison.NodeType,
                    comparison.Left,
                    context);

                return true;
            }

            translation = null;
            return false;
        }

        private static bool IsBooleanConstant(Expression expression)
        {
            return expression.NodeType is Constant or Default &&
                   expression.Type == typeof(bool);
        }

        public ExpressionType NodeType { get; }

        public int TranslationLength => _operandTranslation.TranslationLength + 1;

        public void WriteTo(TranslationWriter writer)
        {
            if (_standaloneBoolean.IsComparisonToTrue)
            {
                _operandTranslation.WriteTo(writer);
                return;
            }

            NegationTranslation.ForNot(_operandTranslation, _context).WriteTo(writer);
        }

        private class StandaloneBoolean
        {
            public StandaloneBoolean(
                Expression boolean,
                ExpressionType @operator,
                Expression comparison)
            {
                Expression = boolean;

                var comparisonValue =
                    comparison.NodeType != Default &&
                    (bool)((ConstantExpression)comparison).Value;

                IsComparisonToTrue =
                    (comparisonValue && @operator == Equal) ||
                   (!comparisonValue && @operator == NotEqual);
            }

            public Expression Expression { get; }

            public bool IsComparisonToTrue { get; }
        }
    }
}