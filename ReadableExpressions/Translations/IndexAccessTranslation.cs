namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class IndexAccessTranslation : ITranslation
    {
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;

        public IndexAccessTranslation(
            ITranslation subject,
            ParameterSetTranslation parameters,
            Type indexValueType)
            : this(subject, parameters)
        {
            NodeType = ExpressionType.Call;
            Type = indexValueType;
        }

        public IndexAccessTranslation(IndexExpression indexAccess, ITranslationContext context)
            : this(indexAccess.Object, indexAccess.Arguments, context)
        {
            NodeType = ExpressionType.Index;
            Type = indexAccess.Type;
        }

        public IndexAccessTranslation(BinaryExpression arrayIndexAccess, ITranslationContext context)
            : this(arrayIndexAccess.Left, new[] { arrayIndexAccess.Right }, context)
        {
            NodeType = ExpressionType.ArrayIndex;
            Type = arrayIndexAccess.Type;
        }

        private IndexAccessTranslation(
            Expression subject,
            ICollection<Expression> arguments,
            ITranslationContext context)
            : this(
                context.GetTranslationFor(subject),
                new ParameterSetTranslation(arguments, context))
        {
        }

        private IndexAccessTranslation(ITranslation subject, ParameterSetTranslation parameters)
        {
            _subject = subject;
            _parameters = parameters;
            TranslationSize = subject.TranslationSize + parameters.TranslationSize + 2;
            FormattingSize = subject.FormattingSize + parameters.FormattingSize;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() 
            => _subject.GetIndentSize() + _parameters.GetIndentSize();

        public int GetLineCount()
        {
            var subjectLineCount = _subject.GetLineCount();
            var parametersLineCount = _parameters.GetLineCount();

            if (subjectLineCount == 1)
            {
                return parametersLineCount > 1 ? parametersLineCount : 1;
            }

            return parametersLineCount > 1
                ? subjectLineCount + parametersLineCount - 1
                : subjectLineCount;
        }

        public void WriteTo(TranslationWriter writer)
        {
            _subject.WriteTo(writer);
            writer.WriteToTranslation('[');
            _parameters.WithoutParentheses().WriteTo(writer);
            writer.WriteToTranslation(']');
        }
    }
}