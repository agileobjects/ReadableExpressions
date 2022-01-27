namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal static class InternalExpressionExtensions
    {
        public static bool IsNamed(this ParameterExpression parameter)
            => !parameter.Name.IsNullOrWhiteSpace();

        public static bool HasReturnType(this Expression expression)
            => expression.Type != typeof(void);

        public static bool IsReturnable(this Expression expression)
        {
            if (!expression.HasReturnType())
            {
                return false;
            }

            switch (expression.NodeType)
            {
                case Block:
                    return ((BlockExpression)expression).IsReturnable();

                case Constant:
                    return !expression.IsComment();

                case Add:
                case AddChecked:
                case Call:
                case Coalesce:
                case Conditional:
                case Convert:
                case ConvertChecked:
                case Default:
                case Divide:
                case Invoke:
                case Label:
                case ListInit:
                case MemberAccess:
                case MemberInit:
                case Multiply:
                case MultiplyChecked:
                case New:
                case NewArrayBounds:
                case NewArrayInit:
                case Parameter:
                case Subtract:
                case SubtractChecked:
                    return true;
            }

            return false;
        }

        public static bool IsReturnable(this BlockExpression block)
            => block.HasReturnType() && block.Result.IsReturnable();

        public static bool IsCapturedValue(
            this Expression expression,
            out object capturedValue,
            out bool isStatic)
        {
            capturedValue = null;
            isStatic = false;
            var capturedMemberAccesses = new List<MemberInfo>();

            while (true)
            {
                switch (expression?.NodeType)
                {
                    case MemberAccess:
                        var memberAccess = (MemberExpression)expression;
                        expression = memberAccess.Expression;
                        capturedMemberAccesses.Add(memberAccess.Member);
                        continue;

                    case Call:
                        var methodCall = (MethodCallExpression)expression;
                        expression = methodCall.Object;
                        capturedMemberAccesses.Add(methodCall.Method);
                        continue;

                    case Constant:
                        var captureConstant = (ConstantExpression)expression;
                        var declaringType = capturedMemberAccesses.LastOrDefault()?.DeclaringType;

                        if (captureConstant.Type != declaringType)
                        {
                            return false;
                        }

                        capturedValue = captureConstant.Value;
                        break;

                    case Convert:
                        expression = ((UnaryExpression)expression).Operand;
                        continue;

                    case null:
                        isStatic = true;
                        break;

                    default:
                        return false;
                }

                if (capturedMemberAccesses.None())
                {
                    return false;
                }

                for (var i = capturedMemberAccesses.Count - 1; i >= 0; --i)
                {
                    capturedValue = capturedMemberAccesses[i].GetValue(capturedValue);
                }

                return true;
            }
        }
    }
}