namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using Translators;

    internal partial class ExpressionTranslatorRegistry
    {
        private readonly List<IExpressionTranslator> _translators = new List<IExpressionTranslator>
        {
            new ArrayLengthExpressionTranslator(),
            new AssignmentExpressionTranslator(),
            new BinaryExpressionTranslator(),
            new BlockExpressionTranslator(),
            new CastExpressionTranslator(),
            new ConstantExpressionTranslator(),
            new InitialisationExpressionTranslator(),
            new DefaultExpressionTranslator(),
            new LambdaExpressionTranslator(),
            new MemberAccessExpressionTranslator(),
            new MethodCallExpressionTranslator(),
            new NegationExpressionTranslator(),
            new NewArrayExpressionTranslator(),
            new NewExpressionTranslator(),
            new ParameterExpressionTranslator(),
        };
    }
}
