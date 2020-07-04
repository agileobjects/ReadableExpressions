namespace AgileObjects.ReadableExpressions.Translations.SourceCode
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Interfaces;
    using static System.Environment;
    using static Formatting.TokenType;

    internal class SummaryTranslation : ITranslatable
    {
        private const string _tripleSlash = "/// ";
        private const string _summaryStart = _tripleSlash + "<summary>";
        private const string _summaryEnd = _tripleSlash + "</summary>";

        private readonly int _lineCount;
        private readonly IList<string> _textLines;

        public SummaryTranslation(IList<string> textLines, ITranslationContext context)
        {
            _lineCount = textLines.Count;

            if (_lineCount == 0)
            {
                return;
            }

            _textLines = textLines.ProjectToArray(line => _tripleSlash + line);

            TranslationSize =
                _summaryStart.Length + NewLine.Length +
                textLines.Sum(line => line.Length + NewLine.Length) +
                _summaryEnd.Length + NewLine.Length;

            FormattingSize =
                GetLineCount() * context.GetFormattingSize(Comment);
        }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => 0;

        public int GetLineCount() => _lineCount + 2;

        public void WriteTo(TranslationWriter writer)
        {
            if (_lineCount == 0)
            {
                return;
            }

            writer.WriteToTranslation(_summaryStart, Comment);
            writer.WriteNewLineToTranslation();

            for (var i = 0; i < _lineCount; ++i)
            {
                writer.WriteToTranslation(_textLines[i], Comment);
                writer.WriteNewLineToTranslation();
            }

            writer.WriteToTranslation(_summaryEnd, Comment);
            writer.WriteNewLineToTranslation();
        }
    }
}