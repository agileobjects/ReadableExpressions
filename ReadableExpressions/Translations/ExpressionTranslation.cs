namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Initialisations;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    /// <summary>
    /// The root class which translates an Expression. Also provides the default
    /// <see cref="ITranslationContext"/> implementation.
    /// </summary>
    public class ExpressionTranslation : ITranslationContext
    {
        private readonly Expression _expression;
        private readonly TranslationSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTranslation"/> class.
        /// </summary>
        /// <param name="expression">The Expression to translate.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use in the translation.</param>
        public ExpressionTranslation(Expression expression, TranslationSettings settings)
            : this(ExpressionAnalysis.For(expression, settings), settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTranslation"/> class.
        /// </summary>
        /// <param name="expressionAnalysis">
        /// The <see cref="ExpressionAnalysis"/> describing the analysed Expression.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use in the translation.</param>
        protected ExpressionTranslation(
            ExpressionAnalysis expressionAnalysis,
            TranslationSettings settings)
        {
            _expression = expressionAnalysis.ResultExpression;
            _settings = settings;
            Analysis = expressionAnalysis;
        }

        /// <inheritdoc />
        public ExpressionAnalysis Analysis { get; }

        #region ITranslationContext Members

        TranslationSettings ITranslationContext.Settings => _settings;

        /// <inheritdoc />
        public virtual ITranslation GetTranslationFor(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression is ICustomTranslationExpression translationExpression)
            {
                return translationExpression.GetTranslation(this);
            }

            switch (expression.NodeType)
            {
                case Decrement:
                case Increment:
                case IsFalse:
                case IsTrue:
                case OnesComplement:
                case PostDecrementAssign:
                case PostIncrementAssign:
                case PreDecrementAssign:
                case PreIncrementAssign:
                case UnaryPlus:
                    return new UnaryTranslation((UnaryExpression)expression, this);

                case Add:
                case AddChecked:
                case And:
                case AndAlso:
                case Coalesce:
                case Divide:
                case Equal:
                case ExclusiveOr:
                case GreaterThan:
                case GreaterThanOrEqual:
                case LeftShift:
                case LessThan:
                case LessThanOrEqual:
                case Modulo:
                case Multiply:
                case MultiplyChecked:
                case NotEqual:
                case Or:
                case OrElse:
                case Power:
                case RightShift:
                case Subtract:
                case SubtractChecked:
                    return BinaryTranslation.For((BinaryExpression)expression, this);

                case AddAssign:
                case AddAssignChecked:
                case AndAssign:
                case Assign:
                case DivideAssign:
                case ExclusiveOrAssign:
                case LeftShiftAssign:
                case ModuloAssign:
                case MultiplyAssign:
                case MultiplyAssignChecked:
                case OrAssign:
                case PowerAssign:
                case RightShiftAssign:
                case SubtractAssign:
                case SubtractAssignChecked:
                    return new AssignmentTranslation((BinaryExpression)expression, this);

                case ArrayIndex:
                    return new IndexAccessTranslation((BinaryExpression)expression, this);

                case ArrayLength:
                    return new ArrayLengthTranslation((UnaryExpression)expression, this);

                case Block:
                    return new BlockTranslation((BlockExpression)expression, this);

                case Call:
                    return MethodCallTranslation.For((MethodCallExpression)expression, this);

                case Conditional:
                    return ConditionalTranslation.For((ConditionalExpression)expression, this);

                case Constant when expression.IsComment():
                    return new CommentTranslation((CommentExpression)expression, this);

                case Constant:
                    return ConstantTranslation.For((ConstantExpression)expression, this);

                case Convert:
                case ConvertChecked:
                case TypeAs:
                case Unbox:
                    return CastTranslation.For((UnaryExpression)expression, this);

                case DebugInfo:
                    return DebugInfoTranslation.For((DebugInfoExpression)expression, this);

                case Default:
                    return DefaultValueTranslation.For(expression, this);

                case Dynamic:
                    return DynamicTranslation.For((DynamicExpression)expression, this);

                case Extension:
                    return new FixedValueTranslation(expression, this);

                case Goto:
                    return GotoTranslation.For((GotoExpression)expression, this);

                case Index:
                    return new IndexAccessTranslation((IndexExpression)expression, this);

                case Invoke:
                    return MethodCallTranslation.For((InvocationExpression)expression, this);

                case Label:
                    return LabelTranslation.For((LabelExpression)expression, this);

                case Lambda:
                    return new LambdaTranslation((LambdaExpression)expression, this);

                case ListInit:
                    return ListInitialisationTranslation.For((ListInitExpression)expression, this);

                case Loop:
                    return new LoopTranslation((LoopExpression)expression, this);

                case MemberAccess:
                    return MemberAccessTranslation.For((MemberExpression)expression, this);

                case MemberInit:
                    return MemberInitialisationTranslation.For((MemberInitExpression)expression, this);

                case Negate:
                case NegateChecked:
                case Not:
                    return new NegationTranslation((UnaryExpression)expression, this);

                case New:
                    return NewingTranslation.For((NewExpression)expression, this);

                case NewArrayBounds:
                    return new NewArrayTranslation((NewArrayExpression)expression, this);

                case NewArrayInit:
                    return ArrayInitialisationTranslation.For((NewArrayExpression)expression, this);

                case Parameter:
                    return ParameterTranslation.For((ParameterExpression)expression, this);

                case Quote:
                    return QuotedLambdaTranslation.For((UnaryExpression)expression, this);

                case RuntimeVariables:
                    return RuntimeVariablesTranslation.For((RuntimeVariablesExpression)expression, this);

                case Switch:
                    return new SwitchTranslation((SwitchExpression)expression, this);

                case Throw:
                    return new ThrowTranslation((UnaryExpression)expression, this);

                case Try:
                    return new TryCatchTranslation((TryExpression)expression, this);

                case TypeEqual:
                    return TypeEqualTranslation.For((TypeBinaryExpression)expression, this);

                case TypeIs:
                    return CastTranslation.For((TypeBinaryExpression)expression, this);

                default:
                    return new FixedValueTranslation(expression, this);
            }
        }

        #endregion

        /// <summary>
        /// Gets the source-code string translation of the given Expression.
        /// </summary>
        /// <returns>The source-code string translation of the given Expression.</returns>
        public string GetTranslation()
        {
            var root = GetTranslationFor(_expression);
            var writer = new TranslationWriter(_settings, root);

            return writer.GetContent();
        }
    }
}