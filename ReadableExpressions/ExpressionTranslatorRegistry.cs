namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
#endif
    using Extensions;
    using Translations;
    using Translators;

    internal class ExpressionTranslatorRegistry
    {
        private readonly Dictionary<ExpressionType, IExpressionTranslator> _translatorsByType;

        public ExpressionTranslatorRegistry()
        {
            var translators = new IExpressionTranslator[]
            {
                default(ArrayLengthExpressionTranslator),
                default(AssignmentExpressionTranslator),
                default(BinaryExpressionTranslator),
                default(BlockExpressionTranslator),
                default(CastExpressionTranslator),
                default(ConditionalExpressionTranslator),
                default(ConstantExpressionTranslator),
                default(DebugInfoExpressionTranslator),
                default(DefaultExpressionTranslator),
                default(DynamicExpressionTranslator),
                default(ExtensionExpressionTranslator),
                default(GotoExpressionTranslator),
                default(IndexAccessExpressionTranslator),
                default(InitialisationExpressionTranslator),
                default(LabelExpressionTranslator),
                default(LambdaExpressionTranslator),
                default(LoopExpressionTranslator),
                default(MemberAccessExpressionTranslator),
                default(MethodCallExpressionTranslator),
                default(NegationExpressionTranslator),
                default(NewArrayExpressionTranslator),
                default(NewExpressionTranslator),
                default(ParameterExpressionTranslator),
                default(QuotedLambdaExpressionTranslator),
                default(RuntimeVariablesExpressionTranslator),
                default(SwitchExpressionTranslator),
                default(TryCatchExpressionTranslator),
                default(TypeEqualExpressionTranslator),
                default(UnaryExpressionTranslator)
            };

            _translatorsByType = translators
                .SelectMany(t => t.NodeTypes.Project(nt => new
                {
                    NodeType = nt,
                    Translator = t
                }))
                .ToDictionary(t => t.NodeType, t => t.Translator);
        }

        public string Translate(
            Expression expression,
            Func<TranslationSettings, TranslationSettings> configuration)
        {
            var context = (expression != null)
                ? TranslationContext.For(expression, Translate, configuration)
                : null;

            var translation = new TranslationTree(expression, context);

            return translation.GetTranslation();

            // return Translate(expression, context);
        }

        private string Translate(Expression expression, TranslationContext context)
        {
            if (expression == null)
            {
                return null;
            }

            return _translatorsByType.TryGetValue(expression.NodeType, out var translator)
                ? translator.Translate(expression, context)
                : expression.ToString();
        }
    }
}