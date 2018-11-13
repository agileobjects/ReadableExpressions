namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal static class RuntimeVariablesTranslation
    {
        public static ITranslation For(RuntimeVariablesExpression runtimeVariables, ITranslationContext context)
            => new ParameterSetTranslation(runtimeVariables.Variables, context).WithNodeType(ExpressionType.RuntimeVariables);
    }
}