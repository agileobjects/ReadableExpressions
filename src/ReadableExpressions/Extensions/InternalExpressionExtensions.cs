namespace AgileObjects.ReadableExpressions.Extensions;

using System.Collections.Generic;
using System.Linq;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using System.Reflection;
using NetStandardPolyfills;
using Translations;
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

    public static bool IsCapture(this Expression expression, out Capture capture)
    {
        capture = new();

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
                    expression = methodCall.GetSubject();
                    capturedMemberAccesses.Add(methodCall.Method);
                    continue;

                case Constant:
                    var captureConstant = (ConstantExpression)expression;

                    if (IsCompileTimeConstant(captureConstant))
                    {
                        return false;
                    }

                    var declaringType = capturedMemberAccesses
                        .LastOrDefault()?.DeclaringType;

                    if (captureConstant.Type != declaringType)
                    {
                        return false;
                    }

                    capture.Object = captureConstant.Value;
                    break;

                case Convert:
                    expression = expression.GetUnaryOperand();
                    continue;

                case null:
                    capture.IsStatic = true;
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
                capture.Object = capturedMemberAccesses[i].GetValue(capture.Object);
            }

            capture.Type = capturedMemberAccesses[0].GetMemberInfoType();
            return true;
        }
    }

    private static bool IsCompileTimeConstant(Expression constant)
    {
        return constant.Type.GetTypeCode() switch
        {
            NetStandardTypeCode.Boolean => true,
            NetStandardTypeCode.Byte => true,
            NetStandardTypeCode.SByte => true,
            NetStandardTypeCode.Char => true,
            NetStandardTypeCode.Decimal => true,
            NetStandardTypeCode.Double => true,
            NetStandardTypeCode.Int16 => true,
            NetStandardTypeCode.UInt16 => true,
            NetStandardTypeCode.Int32 => true,
            NetStandardTypeCode.UInt32 => true,
            NetStandardTypeCode.Int64 => true,
            NetStandardTypeCode.UInt64 => true,
            NetStandardTypeCode.Single => true,
            NetStandardTypeCode.String => true,
            _ => false
        };
    }

    public static Expression GetUnaryOperand(this Expression unary)
        => ((UnaryExpression)unary).Operand;

    public static bool CanBeConvertedToMethodGroup(this Expression argument)
    {
        if (argument.NodeType != Lambda)
        {
            return false;
        }

        var argumentLambda = (LambdaExpression)argument;

        if ((argumentLambda.Body.NodeType != Call) ||
            (argumentLambda.ReturnType != argumentLambda.Body.Type))
        {
            return false;
        }

        if (argumentLambda.Body is not MethodCallExpression methodCall)
        {
            return false;
        }

        IList<Expression> lambdaBodyMethodCallArguments = methodCall.Arguments;

        if (methodCall.Method.IsExtensionMethod())
        {
            lambdaBodyMethodCallArguments = lambdaBodyMethodCallArguments.Skip(1).ToArray();
        }

        if (lambdaBodyMethodCallArguments.Count != argumentLambda.Parameters.Count)
        {
            return false;
        }

        var i = 0;

        var allArgumentTypesMatch = argumentLambda
            .Parameters
            .All(lambdaParameter => lambdaBodyMethodCallArguments[i++] == lambdaParameter);

        return allArgumentTypesMatch;
    }
}