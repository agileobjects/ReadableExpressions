namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Translators;

    internal class ExpressionTranslatorRegistry
    {
        private readonly Dictionary<ExpressionType, IExpressionTranslator> _translatorsByType;

        public ExpressionTranslatorRegistry()
        {
            var negationTranslator = new NegationExpressionTranslator(Translate);
            var memberAccessTranslator = new MemberAccessExpressionTranslator(Translate);
            var defaultTranslator = new DefaultExpressionTranslator(Translate);
            var assignmentTranslator = new AssignmentExpressionTranslator(defaultTranslator, Translate);
            var indexAccessTranslator = new IndexAccessExpressionTranslator(Translate);
            var methodCallTranslator = new MethodCallExpressionTranslator(indexAccessTranslator, Translate);

            var translators = new List<IExpressionTranslator>
            {
                new ArrayLengthExpressionTranslator(Translate),
                assignmentTranslator,
                new BinaryExpressionTranslator(negationTranslator, Translate),
                new BlockExpressionTranslator(Translate),
                new CastExpressionTranslator(Translate),
                new ConditionalExpressionTranslator(Translate),
                new ConstantExpressionTranslator(Translate),
                new DebugInfoExpressionTranslator(Translate),
                defaultTranslator,
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
                negationTranslator,
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

        public string Translate(Expression expression, TranslationContext context)
        {
            if (expression == null)
            {
                return null;
            }

            IExpressionTranslator translator;

            return _translatorsByType.TryGetValue(expression.NodeType, out translator)
                ? translator.Translate(expression, context)
                : expression.ToString();
        }
    }
}