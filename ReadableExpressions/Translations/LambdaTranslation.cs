#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

namespace AgileObjects.ReadableExpressions.Translations
{
    internal class LambdaTranslation : ITranslation
    {
        public LambdaTranslation(LambdaExpression lambda)
        {
        }
    }
}
