namespace AgileObjects.ReadableExpressions.Translations;

using System.Collections.Generic;
using System.Linq;
using Extensions;
using static System.Environment;
using static ExpressionType;
using static GotoExpressionKind;

internal class BlockTranslation :
    INodeTranslation,
    IPotentialMultiStatementTranslatable,
    IPotentialSelfTerminatingTranslation,
    IPotentialGotoTranslation
{
    private readonly IDictionary<ITranslation, ParameterSetTranslation> _variables;
    private readonly bool _hasVariables;
    private readonly IList<BlockStatementTranslation> _statements;
    private readonly int _statementCount;
    private readonly bool _isEmpty;
    private readonly bool _hasGoto;

    public BlockTranslation(BlockExpression block, ITranslationContext context)
    {
        context.Analysis.EnterScope(block);

        _variables = GetVariableDeclarations(block, context);
        _hasVariables = _variables.Count > 0;

        _statements = GetBlockStatements(
            block,
            context,
            out var hasMultiStatementStatement,
            out _hasGoto,
            out _isEmpty);

        IsTerminated = true;

        context.Analysis.ExitScope();

        if (_isEmpty)
        {
            return;
        }

        _statementCount = _statements.Count;
        IsMultiStatement = hasMultiStatementStatement || _statementCount > 1 || _hasVariables;
    }

    private static IDictionary<ITranslation, ParameterSetTranslation> GetVariableDeclarations(
        BlockExpression block,
        ITranslationContext context)
    {
        if (block.Variables.None())
        {
            return EmptyDictionary<ITranslation, ParameterSetTranslation>.Instance;
        }

        var variablesByType = block
            .Variables
            .Filter(context.Analysis.ShouldBeDeclaredInVariableList)
            .GroupBy(v => v.Type)
            .ToArray();

        if (variablesByType.None())
        {
            return EmptyDictionary<ITranslation, ParameterSetTranslation>.Instance;
        }

        return variablesByType.ToDictionary(
            grp => (ITranslation)context.GetTranslationFor(grp.Key),
            grp => ParameterSetTranslation.For(grp.ToList(), context)
                .WithoutParentheses()
                .WithoutTypeNames(context));
    }

    private IList<BlockStatementTranslation> GetBlockStatements(
        BlockExpression block,
        ITranslationContext context,
        out bool hasMultiStatementStatement,
        out bool hasGoto,
        out bool isEmpty)
    {
        var expressions = block.Expressions;
        var expressionCount = expressions.Count;
        var translations = new BlockStatementTranslation[expressionCount];
        var statementIndex = 0;

        hasMultiStatementStatement = false;
        hasGoto = false;

        for (int i = 0, lastExpressionIndex = expressionCount - 1; ; ++i)
        {
            var isFinalStatement = i == lastExpressionIndex;
            var expression = expressions[i];

            if (Include(expression, block, context))
            {
                var statementTranslation = context.Analysis.IsJoinedAssignment(expression)
                    ? new BlockAssignmentStatementTranslation((BinaryExpression)expression, context)
                    : new BlockStatementTranslation(expression, context);

                translations[statementIndex] = statementTranslation;
                hasMultiStatementStatement = hasMultiStatementStatement || statementTranslation.IsMultiStatement;

                ++statementIndex;

                if (!isFinalStatement && statementTranslation.IsTerminal &&
                     HasNoSubsequentGotoTarget(statementIndex, lastExpressionIndex, expressions))
                {
                    isFinalStatement = true;
                    hasGoto = true;
                }

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

        if (statementIndex == 0)
        {
            isEmpty = true;
            return Enumerable<BlockStatementTranslation>.EmptyArray;
        }

        isEmpty = false;

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

    private static bool Include(
        Expression expression,
        BlockExpression block,
        ITranslationContext context)
    {
        switch (expression.NodeType)
        {
            case Label:
                return expression.HasReturnType() ||
                       context.Analysis.IsReferencedByGoto(((LabelExpression)expression).Target);

            case Default when !expression.HasReturnType():
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

        return expression.NodeType != Constant || expression.IsComment();
    }

    private static bool HasNoSubsequentGotoTarget(
        int statementIndex,
        int lastExpressionIndex,
        IList<Expression> expressions)
    {
        for (var j = statementIndex; j < lastExpressionIndex; ++j)
        {
            var subsequentExpression = expressions[j];

            if (subsequentExpression.NodeType == Label)
            {
                return false;
            }
        }

        return true;
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

        return penultimateTranslation.NodeType != Label &&
              !penultimateTranslation.WriteBlankLineAfter() &&
              !penultimateTranslation.Expression.IsComment();
    }

    public ExpressionType NodeType => Block;

    public int TranslationLength
    {
        get
        {
            if (_isEmpty)
            {
                return 0;
            }

            var translationLength =
                _statements.TotalTranslationLength(separator: NewLine);

            if (_hasVariables)
            {
                translationLength += _variables.Sum(parametersByType =>
                    parametersByType.Key.TranslationLength +
                    parametersByType.Value.TranslationLength);
            }

            return translationLength;
        }
    }

    public bool IsMultiStatement { get; }

    public bool IsTerminated { get; private set; }

    public bool HasGoto => _hasGoto;

    public BlockTranslation WithoutTermination()
    {
        _statements[_statementCount - 1].DoNotTerminate = true;
        IsTerminated = false;
        return this;
    }

    public void WriteTo(TranslationWriter writer)
    {
        if (_isEmpty)
        {
            return;
        }

        if (_hasVariables)
        {
            foreach (var parametersByType in _variables)
            {
                var parametersType = parametersByType.Key;
                var parameters = parametersByType.Value;

                parametersType.WriteTo(writer);
                writer.WriteSpaceToTranslation();
                parameters.WriteTo(writer);
                writer.WriteSemiColonToTranslation();
                writer.WriteNewLineToTranslation();
            }

            switch (_statements[0].NodeType)
            {
                case Conditional when !_statements[0].Expression.IsTernary():
                case Switch:
                    writer.WriteNewLineToTranslation();
                    break;
            }
        }

        for (var i = 0; ;)
        {
            _statements[i].WriteTo(writer);

            ++i;

            if (i == _statementCount)
            {
                break;
            }

            writer.WriteNewLineToTranslation();
        }
    }

    private class BlockStatementTranslation :
        INodeTranslation,
        IPotentialMultiStatementTranslatable
    {
        private readonly INodeTranslation _statementTranslation;
        private readonly bool _statementIsUnterminated;
        private bool? _isMultiStatement;
        private bool _writeBlankLineBefore;
        private bool _suppressBlankLineAfter;
        private bool _writeReturnKeyword;

        public BlockStatementTranslation(
            Expression expression,
            ITranslationContext context)
        {
            NodeType = expression.NodeType;
            Expression = expression;
            _statementTranslation = context.GetTranslationFor(expression);
            _statementIsUnterminated = StatementIsUnterminated(expression);
            _writeBlankLineBefore = WriteBlankLineBefore();
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
            return NodeType == Label ||
                  (NodeType == Conditional && !Expression.IsTernary());
        }

        public ExpressionType NodeType { get; }

        public Expression Expression { get; }

        public virtual int TranslationLength => 
            _statementTranslation.TranslationLength + 1;

        public bool IsMultiStatement => 
            _isMultiStatement ??= _statementTranslation.IsMultiStatement();

        public bool IsTerminal => 
            NodeType == Throw || (NodeType == ExpressionType.Goto && ((GotoExpression)Expression).Kind == Return);

        public bool DoNotTerminate { private get; set; }

        public void IsFirstStatement() => _writeBlankLineBefore = false;

        public void IsFinalStatement(bool leaveBlankLineBefore)
        {
            if (_writeBlankLineBefore == false && leaveBlankLineBefore)
            {
                _writeBlankLineBefore = true;
            }

            _suppressBlankLineAfter = true;
        }

        public void WriteReturnKeyword() => _writeReturnKeyword = true;

        public virtual bool HasGoto => 
            _writeReturnKeyword || _statementTranslation.HasGoto();

        public void WriteTo(TranslationWriter writer)
        {
            if ((_writeBlankLineBefore || writer.TranslationQuery(q => q.TranslationEndsWith("};"))) &&
                 !writer.TranslationQuery(q => q.TranslationEndsWithBlankLine()))
            {
                writer.WriteNewLineToTranslation();
            }

            if (_writeReturnKeyword)
            {
                writer.WriteReturnToTranslation();
            }

            WriteStatementTo(writer);

            if (UseFinalBlankLine)
            {
                writer.WriteNewLineToTranslation();
            }
        }

        protected virtual void WriteStatementTo(TranslationWriter writer)
        {
            _statementTranslation.WriteTo(writer);

            if (_statementIsUnterminated && DoNotTerminate == false)
            {
                writer.WriteSemiColonToTranslation();
            }
        }

        private bool UseFinalBlankLine => 
            _suppressBlankLineAfter == false && WriteBlankLineAfter();

        public virtual bool WriteBlankLineAfter()
        {
            return NodeType switch
            {
                Conditional => true,
                Lambda => true,
                Switch => true,
                _ => false
            };
        }
    }

    private class BlockAssignmentStatementTranslation : BlockStatementTranslation
    {
        private const string _var = "var";
        private readonly ITranslation _typeNameTranslation;

        public BlockAssignmentStatementTranslation(
            BinaryExpression assignment,
            ITranslationContext context) :
            base(assignment, context)
        {
            if (UseFullTypeName(assignment, context))
            {
                _typeNameTranslation = context.GetTranslationFor(assignment.Left.Type);
            }
        }

        private static bool UseFullTypeName(
            BinaryExpression assignment,
            ITranslationContext context)
        {
            if (!context.Settings.UseImplicitTypeNames)
            {
                return true;
            }

            if (assignment.Left.Type != assignment.Right.Type ||
                assignment.Right.NodeType == Lambda)
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

        public override int TranslationLength
        {
            get
            {
                var translationLength = base.TranslationLength + _var.Length;

                return _typeNameTranslation != null
                    ? translationLength + _typeNameTranslation.TranslationLength + 2
                    : translationLength;
            }
        }

        public override bool HasGoto => false;

        protected override void WriteStatementTo(TranslationWriter writer)
        {
            if (_typeNameTranslation != null)
            {
                _typeNameTranslation.WriteTo(writer);
            }
            else
            {
                writer.WriteKeywordToTranslation(_var);
            }

            writer.WriteSpaceToTranslation();

            base.WriteStatementTo(writer);
        }

        public override bool WriteBlankLineAfter() => IsMultiStatement;
    }
}