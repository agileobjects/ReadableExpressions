namespace AgileObjects.ReadableExpressions.Translations.SourceCode
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;
    using ReadableExpressions.SourceCode;

    internal class ClassTranslation : ITranslation
    {
        private const string _classString = "class ";

        private readonly ClassExpression _class;
        private readonly ITranslatable _summary;
        private readonly IList<ITranslation> _methods;
        private readonly int _methodCount;

        public ClassTranslation(
            ClassExpression @class,
            ITranslationContext context)
        {
            _class = @class;
            _summary = SummaryTranslation.For(@class.SummaryLines, context);
            _methodCount = @class.Methods.Count;
            _methods = new ITranslation[_methodCount];

            var translationSize =
                _summary.TranslationSize +
                "public ".Length +
                _classString.Length +
                @class.Name.Length +
                6; // <- for opening and closing braces

            var keywordFormattingSize = context.GetKeywordFormattingSize();

            var formattingSize =
               _summary.FormattingSize +
                keywordFormattingSize + // <- for accessibility
                keywordFormattingSize; // <- for 'class'

            for (var i = 0; ;)
            {
                var method = _methods[i] = context.GetTranslationFor(@class.Methods[i]);

                translationSize += method.TranslationSize;
                formattingSize += method.FormattingSize;

                ++i;

                if (i == _methodCount)
                {
                    break;
                }

                translationSize += 2; // <- for new line
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        public ExpressionType NodeType => _class.NodeType;

        public Type Type => _class.Type;

        public int TranslationSize { get; private set; }

        public int FormattingSize { get; private set; }

        public int GetIndentSize()
        {
            var indentSize = 0;

            for (var i = 0; ;)
            {
                indentSize += _methods[i].GetIndentSize();

                ++i;

                if (i == _methodCount)
                {
                    return indentSize;
                }
            }
        }

        public int GetLineCount()
        {
            var lineCount = _summary.GetLineCount();

            for (var i = 0; ;)
            {
                lineCount += _methods[i].GetLineCount();

                ++i;

                if (i == _methodCount)
                {
                    return lineCount;
                }
            }
        }

        public void WriteTo(TranslationWriter writer)
        {
            _summary.WriteTo(writer);

            writer.WriteKeywordToTranslation("public " + _classString);
            writer.WriteTypeNameToTranslation(_class.Name);
            writer.WriteOpeningBraceToTranslation();

            for (var i = 0; ;)
            {
                _methods[i].WriteTo(writer);

                ++i;

                if (i == _methodCount)
                {
                    break;
                }

                writer.WriteNewLineToTranslation();
                writer.WriteNewLineToTranslation();
            }

            writer.WriteClosingBraceToTranslation();
        }
    }
}
