namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class LambdaTranslation : ITranslation, IPotentialMultiStatementTranslatable
    {
        private const string _fatArrow = " => ";

        private readonly ParameterSetTranslation _parameters;
        private readonly CodeBlockTranslation _bodyTranslation;

        public LambdaTranslation(LambdaExpression lambda, ITranslationContext context)
        {
            Type = lambda.Type;
            _parameters = ParameterSetTranslation.For(lambda.Parameters, context);
            _bodyTranslation = context.GetCodeBlockTranslationFor(lambda.Body);

            TranslationSize =
                _parameters.TranslationSize +
                _fatArrow.Length +
                _bodyTranslation.TranslationSize;

            FormattingSize = _parameters.FormattingSize + _bodyTranslation.FormattingSize;

            if (_bodyTranslation.IsMultiStatement == false)
            {
                _bodyTranslation.WithoutTermination();
            }
        }

        public ExpressionType NodeType => ExpressionType.Lambda;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsMultiStatement => _bodyTranslation.IsMultiStatement;

        public int GetIndentSize()
        {
            return _parameters.GetIndentSize() +
                   _bodyTranslation.GetIndentSize();
        }

        public int GetLineCount()
        {
            var parametersLineCount = _parameters.GetLineCount();
            var bodyLineCount = _bodyTranslation.GetLineCount();

            if (parametersLineCount == 1)
            {
                return bodyLineCount > 1 ? bodyLineCount : 1;
            }

            return bodyLineCount > 1
                ? parametersLineCount + bodyLineCount - 1
                : parametersLineCount;
        }

        public void WriteTo(TranslationWriter writer)
        {
            _parameters.WriteTo(writer);

            writer.WriteToTranslation(_bodyTranslation.HasBraces ? " =>" : _fatArrow);

            _bodyTranslation.WriteTo(writer);
        }
    }
}
