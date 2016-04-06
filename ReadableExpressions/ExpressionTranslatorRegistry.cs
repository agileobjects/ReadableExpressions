namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Translators;

    internal class ExpressionTranslatorRegistry
    {
        private readonly Dictionary<ExpressionType, IExpressionTranslator> _translatorsByType;

        public ExpressionTranslatorRegistry()
        {
            var memberAccessTranslator = new MemberAccessExpressionTranslator(Translate);
            var assignmentTranslator = new AssignmentExpressionTranslator(Translate);
            var indexAccessTranslator = new IndexAccessExpressionTranslator(Translate);
            var methodCallTranslator = new MethodCallExpressionTranslator(indexAccessTranslator, Translate);

            var translators = new List<IExpressionTranslator>
            {
                new ArrayLengthExpressionTranslator(Translate),
                new AssignmentExpressionTranslator(Translate),
                new BinaryExpressionTranslator(Translate),
                new BlockExpressionTranslator(Translate),
                new CastExpressionTranslator(Translate),
                new ConditionalExpressionTranslator(Translate),
                new ConstantExpressionTranslator(Translate),
                new DebugInfoExpressionTranslator(Translate),
                new DefaultExpressionTranslator(Translate),
                new DynamicExpressionTranslator(memberAccessTranslator, assignmentTranslator, methodCallTranslator, Translate),
                new ExtensionExpressionTranslator(Translate),
                new GotoExpressionTranslator(Translate),
                indexAccessTranslator,
                new InitialisationExpressionTranslator(methodCallTranslator, Translate),
                new LabelExpressionTranslator(Translate),
                new LambdaExpressionTranslator(Translate),
                new LoopExpressionTranslator(Translate),
                memberAccessTranslator,
                methodCallTranslator,
                new NegationExpressionTranslator(Translate),
                new NewArrayExpressionTranslator(Translate),
                new NewExpressionTranslator(Translate),
                new ParameterExpressionTranslator(Translate),
                new QuotedLambdaExpressionTranslator(Translate),
                new RuntimeVariablesExpressionTranslator(Translate),
                new SwitchExpressionTranslator(Translate),
                new TryCatchExpressionTranslator(Translate),
                new TypeEqualExpressionTranslator(Translate),
                new UnaryExpressionTranslator(Translate)
            };

            _translatorsByType = translators
                .SelectMany(t => t.NodeTypes.Select(nt => new
                {
                    NodeType = nt,
                    Translator = t
                }))
                .ToDictionary(t => t.NodeType, t => t.Translator);
        }

        public string Translate(Expression expression)
        {
            IExpressionTranslator translator;

            if (_translatorsByType.TryGetValue(expression.NodeType, out translator))
            {
                return translator.Translate(expression);
            }

            throw new NotSupportedException(
                $"Unable to translate Expression with NodeType {expression.NodeType}");
        }
    }
}