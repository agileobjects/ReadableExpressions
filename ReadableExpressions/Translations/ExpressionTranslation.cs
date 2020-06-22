namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Initialisations;
    using Interfaces;
    using ReadableExpressions.SourceCode;
    using SourceCode;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif
    using static ReadableExpressions.SourceCode.SourceCodeExpressionType;

    internal class ExpressionTranslation : ITranslationContext
    {
        private readonly TranslationSettings _settings;
        private readonly ExpressionAnalysis _expressionAnalysis;
        private readonly ITranslatable _root;
        private ICollection<ParameterExpression> _declaredOutputParameters;

        public ExpressionTranslation(Expression expression, TranslationSettings settings)
        {
            _settings = settings;
            _expressionAnalysis = ExpressionAnalysis.For(expression, settings);
            _root = GetTranslationFor(expression);
        }

        #region ITranslationContext Members

        TranslationSettings ITranslationContext.Settings => _settings;

        ICollection<ParameterExpression> ITranslationContext.InlineOutputVariables
            => _expressionAnalysis.InlineOutputVariables;

        bool ITranslationContext.ShouldBeDeclaredInline(ParameterExpression parameter)
        {
            var declareInline =
                _expressionAnalysis.InlineOutputVariables.Contains(parameter) &&
                _declaredOutputParameters?.Contains(parameter) != true;

            if (declareInline)
            {
                (_declaredOutputParameters ??= new List<ParameterExpression>()).Add(parameter);
                return true;
            }

            return false;
        }

        ICollection<ParameterExpression> ITranslationContext.JoinedAssignmentVariables
            => _expressionAnalysis.JoinedAssignmentVariables;

        bool ITranslationContext.IsNotJoinedAssignment(Expression expression)
            => _expressionAnalysis.IsNotJoinedAssignment(expression);

        bool ITranslationContext.IsCatchBlockVariable(Expression expression)
            => _expressionAnalysis.IsCatchBlockVariable(expression);

        bool ITranslationContext.IsReferencedByGoto(LabelTarget labelTarget)
            => _expressionAnalysis.IsReferencedByGoto(labelTarget);

        bool ITranslationContext.GoesToReturnLabel(GotoExpression @goto)
            => _expressionAnalysis.GoesToReturnLabel(@goto);

        bool ITranslationContext.IsPartOfMethodCallChain(MethodCallExpression methodCall)
            => _expressionAnalysis.IsPartOfMethodCallChain(methodCall);

        int? ITranslationContext.GetUnnamedVariableNumberOrNull(ParameterExpression variable)
        {
            var variablesOfType = _expressionAnalysis.UnnamedVariablesByType[variable.Type];

            if (variablesOfType.Length == 1)
            {
                return null;
            }

            return Array.IndexOf(variablesOfType, variable, 0) + 1;
        }

        public ITranslation GetTranslationFor(Expression expression)
        {
            if (expression == null)
            {
                return null;
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

                case Constant:
                    return ConstantTranslation.For((ConstantExpression)expression, this);

                case ExpressionType.Convert:
                case ConvertChecked:
                case TypeAs:
                case Unbox:
                    return CastTranslation.For((UnaryExpression)expression, this);

                case DebugInfo:
                    return DebugInfoTranslation.For((DebugInfoExpression)expression, this);

                case Default:
                    return new DefaultValueTranslation(expression, this);

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
                    return new LabelTranslation((LabelExpression)expression, this);

                case Lambda:
                    return new LambdaTranslation((LambdaExpression)expression, this);

                case ListInit:
                    return ListInitialisationTranslation.For((ListInitExpression)expression, this);

                case Loop:
                    return new LoopTranslation((LoopExpression)expression, this);

                case MemberAccess:
                    return new MemberAccessTranslation((MemberExpression)expression, this);

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
                    switch ((SourceCodeExpressionType)expression.NodeType)
                    {
                        case SourceCodeExpressionType.SourceCode:
                            return new SourceCodeTranslation((SourceCodeExpression)expression, this);

                        case Class:
                            return new ClassTranslation((ClassExpression)expression, this);

                        case Method:
                            return new MethodTranslation((MethodExpression)expression, this);

                        case Comment:
                            return new CommentTranslation((CommentExpression)expression, this);
                    }

                    break;
            }

            return new FixedValueTranslation(expression, this);
        }

        #endregion

        public string GetTranslation()
        {
            var writer = new TranslationWriter(_settings, _root);

            return writer.GetContent();
        }
    }
}