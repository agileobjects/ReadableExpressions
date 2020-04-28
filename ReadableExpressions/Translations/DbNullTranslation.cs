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
        public DbNullTranslation(Expression dbNull)
        {
            Type = dbNull.Type;
            EstimatedSize = "DBNull.Value".Length;
        }

        public ExpressionType NodeType => ExpressionType.Constant;
        
        public Type Type { get; }
        
        public int EstimatedSize { get; }
        
        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteTypeNameToTranslation("DBNull");
            buffer.WriteDotToTranslation();
            buffer.WriteToTranslation("Value");
        }
    }
}
