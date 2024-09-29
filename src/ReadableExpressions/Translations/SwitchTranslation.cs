namespace AgileObjects.ReadableExpressions.Translations;

using Extensions;

internal class SwitchTranslation :
    INodeTranslation,
    IPotentialSelfTerminatingTranslation,
    IPotentialMultiStatementTranslatable
{
    private const string _switch = "switch ";
    private const string _case = "case ";

    private readonly INodeTranslation _valueTranslation;
    private readonly INodeTranslation[][] _caseTestValueTranslations;
    private readonly INodeTranslation[] _caseTranslations;
    private readonly int _casesCount;
    private readonly INodeTranslation _defaultCaseTranslation;

    public SwitchTranslation(
        SwitchExpression switchStatement,
        ITranslationContext context)
    {
        _valueTranslation = context.GetTranslationFor(switchStatement.SwitchValue);

        var translationLength = _switch.Length + _valueTranslation.TranslationLength + 4;

        _casesCount = switchStatement.Cases.Count;

        _caseTestValueTranslations = new INodeTranslation[_casesCount][];
        _caseTranslations = new INodeTranslation[_casesCount];

        for (var i = 0; ;)
        {
            var @case = switchStatement.Cases[i];
            var testValueCount = @case.TestValues.Count;

            var caseTestValueTranslations = new INodeTranslation[testValueCount];

            for (var j = 0; ;)
            {
                var caseTestValueTranslation = context.GetTranslationFor(@case.TestValues[j]);
                caseTestValueTranslations[j] = caseTestValueTranslation;

                translationLength += _case.Length + caseTestValueTranslation.TranslationLength + 3;

                ++j;

                if (j == testValueCount)
                {
                    break;
                }

                translationLength += 3;
            }

            _caseTestValueTranslations[i] = caseTestValueTranslations;

            var caseTranslation = GetCaseBodyTranslationOrNull(@case.Body, context);
            _caseTranslations[i] = caseTranslation;
            translationLength += caseTranslation.TranslationLength;

            if (WriteBreak(caseTranslation))
            {
                translationLength += "break;".Length;
            }

            ++i;

            if (i == _casesCount)
            {
                break;
            }
        }

        _defaultCaseTranslation = 
            GetCaseBodyTranslationOrNull(switchStatement.DefaultBody, context);

        if (_defaultCaseTranslation != null)
        {
            translationLength += _defaultCaseTranslation.TranslationLength;

            if (WriteBreak(_defaultCaseTranslation))
            {
                translationLength += "break;".Length;
            }
        }

        TranslationLength = translationLength;
    }

    private static CodeBlockTranslation GetCaseBodyTranslationOrNull(
        Expression caseBody,
        ITranslationContext context)
    {
        return caseBody != null
            ? context.GetCodeBlockTranslationFor(caseBody).WithTermination().WithoutBraces()
            : null;
    }

    public ExpressionType NodeType => ExpressionType.Switch;

    public int TranslationLength { get; }

    public bool IsTerminated => true;

    public bool IsMultiStatement => true;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteControlStatementToTranslation(_switch);
        _valueTranslation.WriteInParentheses(writer);
        writer.WriteOpeningBraceToTranslation();

        for (var i = 0; ;)
        {
            var caseTestValueTranslations = _caseTestValueTranslations[i];

            for (int j = 0, l = caseTestValueTranslations.Length; ;)
            {
                writer.WriteControlStatementToTranslation(_case);
                caseTestValueTranslations[j].WriteTo(writer);
                writer.WriteToTranslation(':');
                writer.WriteNewLineToTranslation();

                ++j;

                if (j == l)
                {
                    break;
                }
            }

            WriteCaseBody(_caseTranslations[i], writer);

            ++i;

            if (i == _casesCount)
            {
                break;
            }

            writer.WriteNewLineToTranslation();
            writer.WriteNewLineToTranslation();
        }

        WriteDefaultIfPresent(writer);

        writer.WriteClosingBraceToTranslation();
    }

    private static void WriteCaseBody(
        ITranslation bodyTranslation,
        TranslationWriter writer)
    {
        writer.Indent();

        bodyTranslation.WriteTo(writer);

        if (WriteBreak(bodyTranslation))
        {
            writer.WriteNewLineToTranslation();
            writer.WriteControlStatementToTranslation("break;");
        }

        writer.Unindent();
    }

    private void WriteDefaultIfPresent(TranslationWriter writer)
    {
        if (_defaultCaseTranslation == null)
        {
            return;
        }

        writer.WriteNewLineToTranslation();
        writer.WriteNewLineToTranslation();
        writer.WriteControlStatementToTranslation("default:");
        writer.WriteNewLineToTranslation();

        WriteCaseBody(_defaultCaseTranslation, writer);
    }

    private static bool WriteBreak(ITranslation caseTranslation) => 
        !caseTranslation.HasGoto();
}