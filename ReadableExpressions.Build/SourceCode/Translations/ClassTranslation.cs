﻿namespace AgileObjects.ReadableExpressions.Build.SourceCode.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using ReadableExpressions.Translations;
    using ReadableExpressions.Translations.Interfaces;

    internal class ClassTranslation : ITranslation
    {
        private const string _classString = "class ";

        private readonly ClassExpression _class;
        private readonly ITranslatable _summary;
        private readonly IList<ITranslation> _interfaces;
        private readonly IList<ITranslation> _methods;
        private readonly int _interfaceCount;
        private readonly int _methodCount;

        public ClassTranslation(
            ClassExpression @class,
            ITranslationContext context)
        {
            _class = @class;
            _summary = SummaryTranslation.For(@class.SummaryLines, context);
            _interfaceCount = @class.Interfaces.Count;

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

            if (_interfaceCount != 0)
            {
                translationSize += 3; // <- for ' : '
                _interfaces = new ITranslation[_interfaceCount];

                for (var i = 0; ;)
                {
                    var @interface = _interfaces[i] = context.GetTranslationFor(@class.Interfaces[i]);
                    translationSize += @interface.TranslationSize;
                    formattingSize += @interface.FormattingSize;

                    ++i;

                    if (i == _interfaceCount)
                    {
                        break;
                    }

                    translationSize += 2; // <- for separator
                }
            }

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

            if (_interfaceCount != 0)
            {
                writer.WriteToTranslation(" : ");

                for (var i = 0; ;)
                {
                    _interfaces[i].WriteTo(writer);

                    ++i;

                    if (i == _interfaceCount)
                    {
                        break;
                    }

                    writer.WriteToTranslation(", ");
                }
            }

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