namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using Translators;

    internal partial class ExpressionTranslatorRegistry
    {
        private readonly List<IExpressionTranslator> _translators = new List<IExpressionTranslator>
        {
            new ArrayLengthExpressionTranslator(),
            new CastExpressionTranslator(),
            new ConstantExpressionTranslator(),
            new LambdaExpressionTranslator(),
            new MemberAccessExpressionTranslator(),
            new MethodCallExpressionTranslator(),
            new NewExpressionTranslator(),
            new ParameterExpressionTranslator(),
        };
    }
}
