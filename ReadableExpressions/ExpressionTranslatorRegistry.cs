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
            var negationTranslator = new NegationExpressionTranslator();
            var memberAccessTranslator = new MemberAccessExpressionTranslator();
            var defaultTranslator = new DefaultExpressionTranslator();
            var assignmentTranslator = new AssignmentExpressionTranslator(defaultTranslator);
            var indexAccessTranslator = new IndexAccessExpressionTranslator();
            var methodCallTranslator = new MethodCallExpressionTranslator(indexAccessTranslator);

            var translators = new List<IExpressionTranslator>
            {
                new ArrayLengthExpressionTranslator(),
                assignmentTranslator,
                new BinaryExpressionTranslator(negationTranslator),
                new BlockExpressionTranslator(),
                new CastExpressionTranslator(),
                new ConditionalExpressionTranslator(),
                new ConstantExpressionTranslator(),
                new DebugInfoExpressionTranslator(),
                defaultTranslator,
#if !NET_STANDARD
                new DynamicExpressionTranslator(memberAccessTranslator, assignmentTranslator, methodCallTranslator),
#endif
                new ExtensionExpressionTranslator(),
                new GotoExpressionTranslator(),
                indexAccessTranslator,
                new InitialisationExpressionTranslator(methodCallTranslator),
                new LabelExpressionTranslator(),
                new LambdaExpressionTranslator(),
                new LoopExpressionTranslator(),
                memberAccessTranslator,
                methodCallTranslator,
                negationTranslator,
                new NewArrayExpressionTranslator(),
                new NewExpressionTranslator(),
                new ParameterExpressionTranslator(),
                new QuotedLambdaExpressionTranslator(),
                new RuntimeVariablesExpressionTranslator(),
                new SwitchExpressionTranslator(),
                new TryCatchExpressionTranslator(),
                new TypeEqualExpressionTranslator(),
                new UnaryExpressionTranslator()
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
            var context = (expression != null)
                ? TranslationContext.For(expression, Translate)
                : null;

            return Translate(expression, context);
        }

        private string Translate(Expression expression, TranslationContext context)
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