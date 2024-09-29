namespace AgileObjects.ReadableExpressions.Translations;

internal static class RuntimeVariablesTranslation
{
    public static INodeTranslation For(
        RuntimeVariablesExpression runtimeVariables,
        ITranslationContext context)
    {
        return ParameterSetTranslation
            .For(runtimeVariables.Variables, context)
            .WithNodeType(ExpressionType.RuntimeVariables);
    }
}