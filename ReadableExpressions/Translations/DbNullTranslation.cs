namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class DbNullTranslation : INodeTranslation
{
    public ExpressionType NodeType => ExpressionType.Constant;

    public int TranslationLength => "DBNull.Value".Length;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteTypeNameToTranslation("DBNull");
        writer.WriteDotToTranslation();
        writer.WriteToTranslation("Value");
    }
}