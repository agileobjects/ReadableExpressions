namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using Translators;

    internal partial class ExpressionTranslatorRegistry
    {
        private readonly List<IExpressionTranslator> _translators = new List<IExpressionTranslator>
        {
            new ArrayLengthExpressionTranslator(),
            new BinaryExpressionTranslator(),
            new CastExpressionTranslator(),
            new ConstantExpressionTranslator(),
            new InitialisationExpressionTranslator(),
            new LambdaExpressionTranslator(),
            new MemberAccessExpressionTranslator(),
            new MethodCallExpressionTranslator(),
            new NegationExpressionTranslator(),
            new NewExpressionTranslator(),
            new ParameterExpressionTranslator(),
        };
    }
}
