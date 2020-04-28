namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class LambdaTranslation : ITranslation, IPotentialMultiStatementTranslatable
    {
        private const string _fatArrow = " => ";

        private readonly ParameterSetTranslation _parameters;
        private readonly CodeBlockTranslation _bodyTranslation;

        public LambdaTranslation(LambdaExpression lambda, ITranslationContext context)
        {
            Type = lambda.Type;
            _parameters = new ParameterSetTranslation(lambda.Parameters, context);
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

        public void WriteTo(TranslationBuffer buffer)
        {
            _parameters.WriteTo(buffer);

            buffer.WriteToTranslation(_bodyTranslation.HasBraces ? " =>" : _fatArrow);

            _bodyTranslation.WriteTo(buffer);
        }
    }
}
