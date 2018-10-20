namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class SwitchTranslation : ITranslation
    {
        private readonly ITranslation _valueTranslation;
        private readonly ITranslation[][] _caseTestValueTranslations;
        private readonly ITranslation[] _caseTranslations;
        private readonly ITranslation _defaultCaseTranslation;

        public SwitchTranslation(SwitchExpression switchStatement, ITranslationContext context)
        {
            _valueTranslation = context.GetTranslationFor(switchStatement.SwitchValue);

            var caseCount = switchStatement.Cases.Count;
            _caseTestValueTranslations = new ITranslation[caseCount][];
            _caseTranslations = new ITranslation[caseCount];

            for (var i = 0; i < caseCount; ++i)
            {
                var @case = switchStatement.Cases[i];
                var testValueCount = @case.TestValues.Count;

                var caseTestValueTranslations = new ITranslation[testValueCount];

                for (var j = 0; j < testValueCount; ++j)
                {
                    caseTestValueTranslations[j] = context.GetTranslationFor(@case.TestValues[j]);
                }

                _caseTestValueTranslations[i] = caseTestValueTranslations;
                _caseTranslations[i] = context.GetCodeBlockTranslationFor(@case.Body).Terminated();
            }

            _defaultCaseTranslation = context.GetTranslationFor(switchStatement.DefaultBody);
        }

        public ExpressionType NodeType => ExpressionType.Switch;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation("switch ");
            _valueTranslation.WriteInParentheses(context);
            context.WriteOpeningBraceToTranslation();

            for (int i = 0, l = _caseTranslations.Length - 1; ; ++i)
            {
                var caseTestValueTranslations = _caseTestValueTranslations[i];

                for (int j = 0, m = caseTestValueTranslations.Length - 1; ; ++j)
                {
                    context.WriteToTranslation("case ");
                    caseTestValueTranslations[j].WriteTo(context);
                    context.WriteToTranslation(':');
                    context.WriteNewLineToTranslation();

                    if (j == m)
                    {
                        break;
                    }
                }

                context.Indent();

                var caseTranslation = _caseTranslations[i];
                caseTranslation.WriteTo(context);

                if (WriteBreak(caseTranslation))
                {
                    context.WriteNewLineToTranslation();
                    context.WriteToTranslation("break;");
                }

                context.Unindent();

                if (i == l)
                {
                    break;
                }

                context.WriteNewLineToTranslation();
                context.WriteNewLineToTranslation();
            }

            context.WriteClosingBraceToTranslation();
        }

        private static bool WriteBreak(ITranslation caseTranslation)
        {
            return caseTranslation.NodeType != ExpressionType.Block;
        }
    }
}