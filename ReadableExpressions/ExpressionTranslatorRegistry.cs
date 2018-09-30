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
                ? TranslationContext.For(expression, OldTranslate, configuration)
                : null;

            return Translate(expression, context);
        }

        private string OldTranslate(Expression expression, TranslationContext context)
            => Translate(expression, context);

        private Translation Translate(Expression expression, TranslationContext context)
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

    internal class Translation
    {
        private readonly char[] _content;
        private int _length;

        public Translation()
        {
            _content = new char[10_000];
        }

        private Translation(string value)
            : this()
        {
            Insert(value);
        }

        public static implicit operator string(Translation translation) => translation?.ToString();

        public static implicit operator Translation(string value) => new Translation(value);

        public Translation Insert(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return this;
            }

            for (var i = _length; i < value.Length; ++i, ++_length)
            {
                _content[i] = value[i];
            }

            return this;
        }

        public override string ToString()
            => (_length != 0) ? new string(_content, 0, _length) : null;
    }
}