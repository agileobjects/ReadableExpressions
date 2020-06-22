namespace AgileObjects.ReadableExpressions.SourceCode
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class SourceCodeExpression
    {
        public static SourceCodeExpression For(
            Expression expression)
        {
            return new SourceCodeExpression();
        }
    }
}
