namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class SwitchTranslation : 
        ITranslation, 
        IPotentialSelfTerminatingTranslatable,
        IPotentialMultiStatementTranslatable
    {
        private const string _switch = "switch ";
        private const string _case = "case ";

        private readonly TranslationSettings _settings;
        private readonly ITranslation _valueTranslation;
        private readonly ITranslation[][] _caseTestValueTranslations;
        private readonly ITranslation[] _caseTranslations;
        private readonly int _casesCount;
        private readonly ITranslation _defaultCaseTranslation;

        public SwitchTranslation(SwitchExpression switchStatement, ITranslationContext context)
        {
            _settings = context.Settings;
            Type = switchStatement.Type;
            _valueTranslation = context.GetTranslationFor(switchStatement.SwitchValue);

            var keywordFormattingSize = context.GetKeywordFormattingSize();
            var translationSize = _switch.Length + _valueTranslation.TranslationSize + 4;

            var formattingSize =
                 keywordFormattingSize + // <- for 'switch'
                _valueTranslation.FormattingSize;

            _casesCount = switchStatement.Cases.Count;

            _caseTestValueTranslations = new ITranslation[_casesCount][];
            _caseTranslations = new ITranslation[_casesCount];

            for (var i = 0; ;)
            {
                var @case = switchStatement.Cases[i];
                var testValueCount = @case.TestValues.Count;

                var caseTestValueTranslations = new ITranslation[testValueCount];

                for (var j = 0; ;)
                {
                    var caseTestValueTranslation = context.GetTranslationFor(@case.TestValues[j]);
                    caseTestValueTranslations[j] = caseTestValueTranslation;

                    translationSize += _case.Length + caseTestValueTranslation.TranslationSize + 3;

                    formattingSize +=
                        keywordFormattingSize + // <- for 'case'
                        caseTestValueTranslation.FormattingSize;

                    ++j;

                    if (j == testValueCount)
                    {
                        break;
                    }

                    translationSize += 3;
                }

                _caseTestValueTranslations[i] = caseTestValueTranslations;

                var caseTranslation = GetCaseBodyTranslationOrNull(@case.Body, context);
                _caseTranslations[i] = caseTranslation;
                translationSize += caseTranslation.TranslationSize;
                formattingSize += caseTranslation.FormattingSize;

                if (WriteBreak(caseTranslation))
                {
                    translationSize += "break;".Length;
                    formattingSize += keywordFormattingSize;
                }

                ++i;

                if (i == _casesCount)
                {
                    break;
                }
            }

            _defaultCaseTranslation = GetCaseBodyTranslationOrNull(switchStatement.DefaultBody, context);

            if (_defaultCaseTranslation != null)
            {
                translationSize += _defaultCaseTranslation.TranslationSize;
                formattingSize += _defaultCaseTranslation.FormattingSize;

                if (WriteBreak(_defaultCaseTranslation))
                {
                    translationSize += "break;".Length;
                    formattingSize += keywordFormattingSize;
                }
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        private static CodeBlockTranslation GetCaseBodyTranslationOrNull(Expression caseBody, ITranslationContext context)
            => (caseBody != null) ? context.GetCodeBlockTranslationFor(caseBody).WithTermination().WithoutBraces() : null;

        public ExpressionType NodeType => ExpressionType.Switch;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsTerminated => true;

        public bool IsMultiStatement => true;

        public int GetIndentSize()
        {
            var indentSize = 0;
            var indentLength = _settings.IndentLength;

            for (var i = 0; ;)
            {
                indentSize += _caseTestValueTranslations[i].Length * indentLength;

                var caseTranslation = _caseTranslations[i];
                indentSize += caseTranslation.GetLineCount() * indentLength * 2;

                if (WriteBreak(caseTranslation))
                {
                    indentSize += indentLength * 2;
                }

                ++i;

                if (i == _casesCount)
                {
                    break;
                }
            }

            if (_defaultCaseTranslation != null)
            {
                indentSize += indentLength;
                indentSize += _defaultCaseTranslation.GetLineCount() * indentLength * 2;

                if (WriteBreak(_defaultCaseTranslation))
                {
                    indentSize += indentLength * 2;
                }
            }

            return indentSize;
        }

        public int GetLineCount()
        {
            var lineCount = 3;

            for (var i = 0; ;)
            {
                lineCount += _caseTestValueTranslations[i].Length;

                var caseTranslation = _caseTranslations[i];
                lineCount += caseTranslation.GetLineCount();

                if (WriteBreak(caseTranslation))
                {
                    lineCount += 1;
                }

                ++i;

                if (i == _casesCount)
                {
                    break;
                }

                lineCount += 1;
            }

            if (_defaultCaseTranslation != null)
            {
                lineCount += _defaultCaseTranslation.GetLineCount() + 2;

                if (WriteBreak(_defaultCaseTranslation))
                {
                    lineCount += 1;
                }
            }

            return lineCount;
        }

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

        private static void WriteCaseBody(ITranslation bodyTranslation, TranslationWriter writer)
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

        private static bool WriteBreak(ITranslatable caseTranslation) 
            => !caseTranslation.HasGoto();
    }
}