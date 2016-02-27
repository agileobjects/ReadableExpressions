namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Translators;

    internal class ExpressionTranslatorRegistry : IExpressionTranslatorRegistry
    {
        private readonly Dictionary<ExpressionType, IExpressionTranslator> _translatorsByType;

        public ExpressionTranslatorRegistry()
        {
            var memberAccessTranslator = new MemberAccessExpressionTranslator(this);
            var assignmentTranslator = new AssignmentExpressionTranslator(this);
            var indexAccessTranslator = new IndexAccessExpressionTranslator(this);
            var methodCallTranslator = new MethodCallExpressionTranslator(indexAccessTranslator, this);

            var translators = new List<IExpressionTranslator>
            {
                new ArrayLengthExpressionTranslator(this),
                new AssignmentExpressionTranslator(this),
                new BinaryExpressionTranslator(this),
                new BlockExpressionTranslator(this),
                new CastExpressionTranslator(this),
                new ConditionalExpressionTranslator(this),
                new ConstantExpressionTranslator(this),
                new DebugInfoExpressionTranslator(this),
                new DefaultExpressionTranslator(this),
                new DynamicExpressionTranslator(memberAccessTranslator, assignmentTranslator, methodCallTranslator, this),
                new ExtensionExpressionTranslator(this),
                new GotoExpressionTranslator(this),
                indexAccessTranslator,
                new InitialisationExpressionTranslator(methodCallTranslator, this),
                new LabelExpressionTranslator(this),
                new LambdaExpressionTranslator(this),
                new LoopExpressionTranslator(this),
                memberAccessTranslator,
                methodCallTranslator,
                new NegationExpressionTranslator(this),
                new NewArrayExpressionTranslator(this),
                new NewExpressionTranslator(this),
                new ParameterExpressionTranslator(this),
                new QuotedLambdaExpressionTranslator(this),
                new RuntimeVariablesExpressionTranslator(this),
                new SwitchExpressionTranslator(this),
                new TryCatchExpressionTranslator(this),
                new TypeEqualExpressionTranslator(this),
                new UnaryExpressionTranslator(this)
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