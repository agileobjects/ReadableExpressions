namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Text;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class TranslationTree : ITranslationContext, ITranslationQuery
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

        IEnumerable<ParameterExpression> ITranslationContext.JoinedAssignmentVariables
            => _context.JoinedAssignmentVariables;

        bool ITranslationContext.IsNotJoinedAssignment(Expression expression)
            => _context.IsNotJoinedAssignment(expression);

        bool ITranslationContext.IsReferencedByGoto(LabelTarget labelTarget)
            => _context.IsReferencedByGoto(labelTarget);

        bool ITranslationContext.GoesToReturnLabel(GotoExpression @goto)
            => _context.GoesToReturnLabel(@goto);

        int? ITranslationContext.GetUnnamedVariableNumber(ParameterExpression variable)
            => _context.GetUnnamedVariableNumber(variable);

        ITranslation ITranslationContext.GetTranslationFor(Type type) => new TypeNameTranslation(type);

        ITranslation ITranslationContext.GetTranslationFor(Expression expression)
            => GetTranslationFor(expression);

        CodeBlockTranslation ITranslationContext.GetCodeBlockTranslationFor(Expression expression)
            => new CodeBlockTranslation(GetTranslationFor(expression));

        bool ITranslationContext.TranslationQuery(Func<ITranslationQuery, bool> predicate)
            => predicate.Invoke(this);

        void ITranslationContext.Indent()
        {
            _currentIndent += Constants.Indent.Length;

            if (_writeIndent == false)
            {
                _writeIndent = true;
            }
        }

        void ITranslationContext.Unindent()
        {
            _currentIndent -= Constants.Indent.Length;
        }

        void ITranslationContext.WriteNewLineToTranslation()
        {
            _content.Append(Environment.NewLine);

            if (_currentIndent != 0)
            {
                _writeIndent = true;
            }
        }

        public void WriteToTranslation(char character)
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

        #region ITranslationQuery

        bool ITranslationQuery.TranslationEndsWith(char character)
        {
            for (var i = _content.Length - 1; i > -1; --i)
            {
                var contentCharacter = _content[i];

                if (char.IsWhiteSpace(contentCharacter))
                {
                    continue;
                }

                return contentCharacter == character;
            }

            return false;
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
                    return new BlockTranslation((BlockExpression)expression, this);

                case Call:
                    return new MethodCallTranslation((MethodCallExpression)expression, this);

                case Conditional:
                    return new ConditionalTranslation((ConditionalExpression)expression, this);

                case Constant:
                    return new ConstantTranslation((ConstantExpression)expression, this);

                case ExpressionType.Convert:
                case ConvertChecked:
                case TypeAs:
                case Unbox:
                    return new CastTranslation((UnaryExpression)expression, this);

                case DebugInfo:
                    break;

                case Default:
                    return new DefaultValueTranslation(expression, this);

                case Dynamic:
                    break;
                case Extension:
                    break;
                case Goto:
                    return new GotoTranslation((GotoExpression)expression, this);

                case Index:
                    return new IndexAccessTranslation((IndexExpression)expression, this);

                case Invoke:
                    break;

                case Label:
                    return new LabelTranslation((LabelExpression)expression, this);

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

                case Parameter:
                    return new ParameterTranslation((ParameterExpression)expression);

                case Quote:
                    break;
                case RuntimeVariables:
                    break;
                case Switch:
                    return new SwitchTranslation((SwitchExpression)expression, this);

                case Throw:
                    break;
                case Try:
                    break;

                case TypeEqual:
                    return new TypeEqualTranslation((TypeBinaryExpression)expression, this);

                case TypeIs:
                    return new CastTranslation((TypeBinaryExpression)expression, this);
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