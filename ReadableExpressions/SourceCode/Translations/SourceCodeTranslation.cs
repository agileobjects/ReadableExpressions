namespace AgileObjects.ReadableExpressions.SourceCode.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
    using ReadableExpressions.Translations;
    using ReadableExpressions.Translations.Interfaces;
    using SourceCode;

    internal class SourceCodeTranslation : ITranslation
    {
        private const string _using = "using ";
        private const string _namespace = "namespace ";

        private readonly IList<string> _namespaces;
        private readonly int _namespaceCount;
        private readonly bool _hasNamespace;
        private readonly SourceCodeExpression _sourceCode;
        private readonly IList<ITranslation> _classes;
        private readonly int _classCount;

        public SourceCodeTranslation(
            SourceCodeExpression sourceCode,
            ISourceCodeTranslationContext context)
        {
            _namespaces = context.RequiredNamespaces;
            _namespaceCount = context.RequiredNamespaces.Count;
            _hasNamespace = !sourceCode.Namespace.IsNullOrWhiteSpace();
            _sourceCode = sourceCode;
            _classCount = sourceCode.Classes.Count;
            _classes = new ITranslation[_classCount];

            var translationSize = 6; // <- for opening and closing braces

            if (_hasNamespace)
            {
                translationSize += _namespace.Length + sourceCode.Namespace.Length;
            }

            var keywordFormattingSize = context.GetKeywordFormattingSize();

            var formattingSize =
                _hasNamespace ? keywordFormattingSize : 0;

            if (_namespaceCount != 0)
            {
                for (var i = 0; ;)
                {
                    translationSize += _namespaces[i].Length;
                    formattingSize += keywordFormattingSize; // <- for using

                    ++i;

                    if (i == _namespaceCount)
                    {
                        break;
                    }

                    translationSize += 2; // <- for new line
                }
            }

            for (var i = 0; ;)
            {
                var @class = _classes[i] = context.GetTranslationFor(sourceCode.Classes[i]);

                translationSize += @class.TranslationSize;
                formattingSize += @class.FormattingSize;

                ++i;

                if (i == _classCount)
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
                indentSize += _classes[i].GetIndentSize();

                ++i;

                if (i == _classCount)
                {
                    return indentSize;
                }
            }
        }

        public int GetLineCount()
        {
            var lineCount =
                _namespaceCount +
                3; // <- for braces and namespace declaration

            if (_namespaceCount > 0)
            {
                ++lineCount; // <- for gap between namespaces and namespace declaration
            }

            for (var i = 0; ;)
            {
                lineCount += _classes[i].GetLineCount();

                ++i;

                if (i == _classCount)
                {
                    return lineCount;
                }
            }
        }

        public void WriteTo(TranslationWriter writer)
        {
            if (_namespaceCount != 0)
            {
                for (var i = 0; i < _namespaceCount; ++i)
                {
                    writer.WriteKeywordToTranslation(_using);
                    writer.WriteToTranslation(_namespaces[i]);
                    writer.WriteToTranslation(';');
                    writer.WriteNewLineToTranslation();
                }

                writer.WriteNewLineToTranslation();
            }

            if (_hasNamespace)
            {
                writer.WriteKeywordToTranslation(_namespace);
                writer.WriteToTranslation(_sourceCode.Namespace);
                writer.WriteOpeningBraceToTranslation();
            }

            for (var i = 0; ;)
            {
                _classes[i].WriteTo(writer);

                ++i;

                if (i == _classCount)
                {
                    break;
                }

                writer.WriteNewLineToTranslation();
                writer.WriteNewLineToTranslation();
            }

            if (_hasNamespace)
            {
                writer.WriteClosingBraceToTranslation();
            }
        }
    }
}
