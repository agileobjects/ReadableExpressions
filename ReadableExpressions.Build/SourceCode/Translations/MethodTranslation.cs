namespace AgileObjects.ReadableExpressions.Build.SourceCode.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using ReadableExpressions.Translations;
    using ReadableExpressions.Translations.Interfaces;
    using ReadableExpressions.Translations.Reflection;

    internal class MethodTranslation : ITranslation
    {
        private readonly MethodExpression _method;
        private readonly ITranslatable _summary;
        private readonly ITranslatable _definitionTranslation;
        private readonly ITranslation _bodyTranslation;

        public MethodTranslation(MethodExpression method, ITranslationContext context)
        {
            _method = method;
            _summary = SummaryTranslation.For(method.SummaryLines, context);

            _definitionTranslation = new MethodDefinitionTranslation(
                method.Method,
                context.Settings);

            var bodyCodeBlock = context
                .GetCodeBlockTranslationFor(method.Body)
                .WithBraces()
                .WithReturnKeyword()
                .WithTermination();

            _bodyTranslation = bodyCodeBlock;

            TranslationSize =
                _summary.TranslationSize +
                _definitionTranslation.TranslationSize +
                _bodyTranslation.TranslationSize;

            FormattingSize =
                _summary.FormattingSize +
                _definitionTranslation.FormattingSize +
                _bodyTranslation.FormattingSize;
        }

        public ExpressionType NodeType => _method.NodeType;

        public Type Type => _method.Type;

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize()
        {
            return
                _definitionTranslation.GetIndentSize() +
                _bodyTranslation.GetIndentSize();
        }

        public int GetLineCount()
        {
            return
                _definitionTranslation.GetLineCount() +
                _bodyTranslation.GetLineCount();
        }

        public void WriteTo(TranslationWriter writer)
        {
            _summary.WriteTo(writer);
            _definitionTranslation.WriteTo(writer);
            _bodyTranslation.WriteTo(writer);
        }
    }
}
