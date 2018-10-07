namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Text;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using static Constants;

    internal class TranslationTree : ITranslationContext
    {
        private readonly TranslationContext _context;
        private readonly ITranslation _root;
        private readonly StringBuilder _content;
        private int _currentIndent;
        private bool _writeIndent;

        public TranslationTree(Expression expression, TranslationContext context)
        {
            _context = context;
            _root = GetTranslationFor(expression);
            _content = new StringBuilder(_root.EstimatedSize);
        }

        #region ITranslationContext Members

        TranslationSettings ITranslationContext.Settings => _context.Settings;

        int? ITranslationContext.GetUnnamedVariableNumber(ParameterExpression variable)
            => _context.GetUnnamedVariableNumber(variable);

        ITranslation ITranslationContext.GetTranslationFor(Type type) => new TypeNameTranslation(type);

        ITranslation ITranslationContext.GetTranslationFor(Expression expression)
            => GetTranslationFor(expression);

        void ITranslationContext.Indent()
        {
            _currentIndent += Indent.Length;

            if (_writeIndent == false)
            {
                _writeIndent = true;
            }
        }

        void ITranslationContext.Unindent()
        {
            _currentIndent -= Indent.Length;
        }

        void ITranslationContext.WriteNewLineToTranslation()
        {
            _content.Append(Environment.NewLine);

            if (_currentIndent != 0)
            {
                _writeIndent = true;
            }
        }

        void ITranslationContext.WriteToTranslation(char character)
        {
            WriteIndentIfRequired();
            _content.Append(character);
        }

        void ITranslationContext.WriteToTranslation(string stringValue)
        {
            WriteIndentIfRequired();
            _content.Append(stringValue);
        }

        void ITranslationContext.WriteToTranslation(int intValue)
        {
            WriteIndentIfRequired();
            _content.Append(intValue);
        }

        void ITranslationContext.WriteToTranslation(object value)
        {
            WriteIndentIfRequired();
            _content.Append(value);
        }

        private void WriteIndentIfRequired()
        {
            if (_writeIndent)
            {
                _content.Append(' ', _currentIndent);
                _writeIndent = false;
            }
        }

        #endregion

        private ITranslation GetTranslationFor(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            switch (expression.NodeType)
            {
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
                    return new BinaryTranslation((BinaryExpression)expression, this);

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
                    break;
                case Call:
                    return new MethodCallTranslation((MethodCallExpression)expression, this);

                case Conditional:
                    break;
                case Constant:
                    return new ConstantTranslation((ConstantExpression)expression, this);

                case ExpressionType.Convert:
                case ConvertChecked:
                case TypeAs:
                case Unbox:
                    return new CastTranslation((UnaryExpression)expression, this);

                case DebugInfo:
                    break;
                case Decrement:
                    break;

                case Default:
                    return new DefaultValueTranslation(expression, this);

                case Dynamic:
                    break;
                case Extension:
                    break;
                case Goto:
                    break;
                case Increment:
                    break;
                case Index:
                    return new IndexAccessTranslation((IndexExpression)expression, this);

                case Invoke:
                    break;
                case IsFalse:
                    break;
                case IsTrue:
                    break;
                case Label:
                    break;
                case Lambda:
                    return new LambdaTranslation((LambdaExpression)expression, this);

                case ListInit:
                    return new InitialisationTranslation((ListInitExpression)expression, this);

                case Loop:
                    break;
                case MemberAccess:
                    return new MemberAccessTranslation((MemberExpression)expression, this);

                case MemberInit:
                    break;

                case Negate:
                case NegateChecked:
                case Not:
                    return new NegationTranslation((UnaryExpression)expression, this);

                case New:
                    return new NewingTranslation((NewExpression)expression, this);

                case NewArrayBounds:
                    break;
                case NewArrayInit:
                    break;

                case OnesComplement:
                    break;
                case Parameter:
                    return new ParameterTranslation((ParameterExpression)expression);

                case PostDecrementAssign:
                    break;
                case PostIncrementAssign:
                    break;
                case PreDecrementAssign:
                    break;
                case PreIncrementAssign:
                    break;
                case Quote:
                    break;
                case RuntimeVariables:
                    break;
                case Switch:
                    break;
                case Throw:
                    break;
                case Try:
                    break;
                case TypeEqual:
                    break;
                case TypeIs:
                    break;
                case UnaryPlus:
                    break;
            }

            throw new ArgumentOutOfRangeException(expression.NodeType.ToString());
        }

        public string GetTranslation()
        {
            _root.WriteTo(this);

            return (_content.Length > 0) ? _content.ToString() : null;
        }
    }
}