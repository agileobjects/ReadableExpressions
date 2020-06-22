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

    internal class SourceCodeTranslation : ITranslation
    {
        private const string _namespace = "namespace ";

        private readonly SourceCodeExpression _sourceCode;
        private readonly IList<ITranslation> _elements;
        private readonly int _elementCount;

        public SourceCodeTranslation(
            SourceCodeExpression sourceCode,
            ITranslationContext context)
        {
            _sourceCode = sourceCode;
            _elementCount = sourceCode.Elements.Count;
            _elements = new ITranslation[_elementCount];

            var translationSize =
                _namespace.Length +
                sourceCode.Namespace.Length +
                6; // <- for opening and closing braces

            var formattingSize = context.GetKeywordFormattingSize(); // <- for 'namespace'

            for (var i = 0; ;)
            {
                var element = _elements[i] = context.GetTranslationFor(sourceCode.Elements[i]);

                translationSize += element.TranslationSize;
                formattingSize += element.FormattingSize;

                ++i;

                if (i == _elementCount)
                {
                    break;
                }

                translationSize += 2; // <- for new line
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        public ExpressionType NodeType => _sourceCode.NodeType;

        public Type Type => _sourceCode.Type;

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize()
        {
            var indentSize = 0;

            for (var i = 0; ;)
            {
                indentSize += _elements[i].GetIndentSize();

                ++i;

                if (i == _elementCount)
                {
                    return indentSize;
                }
            }
        }

        public int GetLineCount()
        {
            var lineCount = 0;

            for (var i = 0; ;)
            {
                lineCount += _elements[i].GetLineCount();

                ++i;

                if (i == _elementCount)
                {
                    return lineCount;
                }
            }
        }

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_namespace);
            writer.WriteToTranslation(_sourceCode.Namespace);
            writer.WriteOpeningBraceToTranslation();

            for (var i = 0; ;)
            {
                _elements[i].WriteTo(writer);

                ++i;

                if (i == _elementCount)
                {
                    break;
                }

                writer.WriteNewLineToTranslation();
            }

            writer.WriteClosingBraceToTranslation();
        }
    }
}
