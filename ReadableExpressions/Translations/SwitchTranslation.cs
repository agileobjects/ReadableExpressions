namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class SwitchTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly ITranslation _valueTranslation;
        private readonly ITranslation[][] _caseTestValueTranslations;
        private readonly ITranslation[] _caseTranslations;
        private readonly ITranslation _defaultCaseTranslation;

        public SwitchTranslation(SwitchExpression switchStatement, ITranslationContext context)
        {
            Type = switchStatement.Type;
            _valueTranslation = context.GetTranslationFor(switchStatement.SwitchValue);

            var estimatedSize = _valueTranslation.EstimatedSize;
            var caseCount = switchStatement.Cases.Count;

            _caseTestValueTranslations = new ITranslation[caseCount][];
            _caseTranslations = new ITranslation[caseCount];

            for (var i = 0; ;)
            {
                var @case = switchStatement.Cases[i];
                var testValueCount = @case.TestValues.Count;

                var caseTestValueTranslations = new ITranslation[testValueCount];

                for (var j = 0; ;)
                {
                    var caseTestValueTranslation = context.GetTranslationFor(@case.TestValues[j]);
                    caseTestValueTranslations[j] = caseTestValueTranslation;
                    estimatedSize += caseTestValueTranslation.EstimatedSize;

                    ++j;

                    if (j == testValueCount)
                    {
                        break;
                    }
                }

                _caseTestValueTranslations[i] = caseTestValueTranslations;
                _caseTranslations[i] = GetCaseBodyTranslationOrNull(@case.Body, context);

                ++i;

                if (i == caseCount)
                {
                    break;
                }
            }

            _defaultCaseTranslation = GetCaseBodyTranslationOrNull(switchStatement.DefaultBody, context);

            if (_defaultCaseTranslation != null)
            {
                estimatedSize += _defaultCaseTranslation.EstimatedSize;
            }

            EstimatedSize = estimatedSize;
        }

        private static CodeBlockTranslation GetCaseBodyTranslationOrNull(Expression caseBody, ITranslationContext context)
            => (caseBody != null) ? context.GetCodeBlockTranslationFor(caseBody).WithTermination().WithoutBraces() : null;

        public ExpressionType NodeType => ExpressionType.Switch;
        
        public Type Type { get; }

        public int EstimatedSize { get; }

        public bool IsTerminated => true;

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteControlStatementToTranslation("switch ");
            _valueTranslation.WriteInParentheses(buffer);
            buffer.WriteOpeningBraceToTranslation();

            for (int i = 0, l = _caseTranslations.Length - 1; ; ++i)
            {
                var caseTestValueTranslations = _caseTestValueTranslations[i];

                for (int j = 0, m = caseTestValueTranslations.Length - 1; ; ++j)
                {
                    buffer.WriteControlStatementToTranslation("case ");
                    caseTestValueTranslations[j].WriteTo(buffer);
                    buffer.WriteToTranslation(':');
                    buffer.WriteNewLineToTranslation();

                    if (j == m)
                    {
                        break;
                    }
                }

                WriteCaseBody(_caseTranslations[i], buffer);

                if (i == l)
                {
                    break;
                }

                buffer.WriteNewLineToTranslation();
                buffer.WriteNewLineToTranslation();
            }

            WriteDefaultIfPresent(buffer);

            buffer.WriteClosingBraceToTranslation();
        }

        private static void WriteCaseBody(ITranslation bodyTranslation, TranslationBuffer buffer)
        {
            buffer.Indent();

            bodyTranslation.WriteTo(buffer);

            if (WriteBreak(bodyTranslation))
            {
                buffer.WriteNewLineToTranslation();
                buffer.WriteControlStatementToTranslation("break;");
            }

            buffer.Unindent();
        }

        private void WriteDefaultIfPresent(TranslationBuffer buffer)
        {
            if (_defaultCaseTranslation == null)
            {
                return;
            }

            buffer.WriteNewLineToTranslation();
            buffer.WriteNewLineToTranslation();
            buffer.WriteControlStatementToTranslation("default:");
            buffer.WriteNewLineToTranslation();

            WriteCaseBody(_defaultCaseTranslation, buffer);
        }

        private static bool WriteBreak(ITranslation caseTranslation)
            => !((caseTranslation is IPotentialGotoTranslatable gotoTranslatable) && gotoTranslatable.HasGoto);
    }
}