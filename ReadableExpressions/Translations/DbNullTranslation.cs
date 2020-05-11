namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class DbNullTranslation : ITranslation
    {
        public DbNullTranslation(Expression dbNull, ITranslationContext context)
        {
            Type = dbNull.Type;
            TranslationSize = "DBNull.Value".Length;
            FormattingSize = context.GetTypeNameFormattingSize();
        }

        public ExpressionType NodeType => ExpressionType.Constant;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetLineCount() => 1;

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteTypeNameToTranslation("DBNull");
            buffer.WriteDotToTranslation();
            buffer.WriteToTranslation("Value");
        }
    }
}
