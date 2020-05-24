namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Text.RegularExpressions;
    using Extensions;
    using Formatting;
    using Interfaces;
    using NetStandardPolyfills;

    internal static class DynamicTranslation
    {
        public static ITranslation For(DynamicExpression dynamicExpression, ITranslationContext context)
        {
            var args = new DynamicTranslationArgs(dynamicExpression, context);

            if (DynamicMemberAccessTranslation.TryGetTranslation(args, out var translation))
            {
                return translation;
            }

            if (DynamicMemberWriteTranslation.TryGetTranslation(args, out translation))
            {
                return translation;
            }

            if (DynamicMethodCallTranslation.TryGetTranslation(args, out translation))
            {
                return translation;
            }

            return new FixedValueTranslation(
                ExpressionType.Dynamic,
                args.OperationDescription,
                dynamicExpression.Type,
                TokenType.Default,
                context);
        }

        private class DynamicTranslationArgs
        {
            private readonly DynamicExpression _dynamicExpression;

            public DynamicTranslationArgs(DynamicExpression dynamicExpression, ITranslationContext context)
            {
                _dynamicExpression = dynamicExpression;
                OperationDescription = dynamicExpression.ToString();
                Context = context;
            }

            public string OperationDescription { get; }

            public ITranslationContext Context { get; }

            public Expression FirstArgument => Arguments.First();

            public IList<Expression> Arguments => _dynamicExpression.Arguments;

            public Expression LastArgument => Arguments.Last();

            public Type ExpressionType => _dynamicExpression.Type;

            public bool IsMatch(Regex matcher, out Match match)
            {
                match = matcher.Match(OperationDescription);

                return match.Success;
            }
        }

        private static class DynamicMemberAccessTranslation
        {
            private static readonly Regex _matcher = new Regex(@"^GetMember (?<MemberName>[^\(]+)\(");

            public static bool TryGetTranslation(DynamicTranslationArgs args, out ITranslation translation)
            {
                if (!args.IsMatch(_matcher, out var match))
                {
                    translation = null;
                    return false;
                }

                translation = GetTranslation(args, match);
                return true;
            }

            public static ITranslation GetTranslation(DynamicTranslationArgs args, Match match)
            {
                var subject = args.Context.GetTranslationFor(args.FirstArgument);
                var memberName = match.Groups["MemberName"].Value;

                return new MemberAccessTranslation(subject, memberName, args.ExpressionType, args.Context);
            }
        }

        private static class DynamicMemberWriteTranslation
        {
            private static readonly Regex _matcher = new Regex(@"^SetMember (?<MemberName>[^\(]+)\(");

            public static bool TryGetTranslation(DynamicTranslationArgs args, out ITranslation translation)
            {
                if (!args.IsMatch(_matcher, out var match))
                {
                    translation = null;
                    return false;
                }

                var targetTranslation = DynamicMemberAccessTranslation.GetTranslation(args, match);

                translation = new AssignmentTranslation(
                    ExpressionType.Assign,
                    targetTranslation,
                    args.LastArgument,
                    args.Context);

                return true;
            }
        }

        private static class DynamicMethodCallTranslation
        {
            private static readonly Regex _matcher = new Regex(@"^Call (?<MethodName>[^\(]+)\(");

            public static bool TryGetTranslation(DynamicTranslationArgs args, out ITranslation translation)
            {
                if (!args.IsMatch(_matcher, out var match))
                {
                    translation = null;
                    return false;
                }

                var subjectObject = args.FirstArgument;
                var subjectTranslation = args.Context.GetTranslationFor(subjectObject);
                var methodName = match.Groups["MethodName"].Value;
                var methodInfo = subjectObject.Type.GetPublicMethod(methodName);
                var methodArguments = args.Arguments.Skip(1).ToArray();

                var method = GetMethod(
                    methodName,
                    methodInfo,
                    methodArguments,
                    args.ExpressionType);

                translation = MethodCallTranslation.ForDynamicMethodCall(
                    subjectTranslation,
                    method,
                    methodArguments,
                    args.Context);

                return true;
            }

            private static IMethod GetMethod(
                string methodName,
                MethodInfo method,
                Expression[] methodArguments,
                Type methodReturnType)
            {
                if (method == null)
                {
                    return new MissingMethod(methodName);
                }

                return new BclMethodWrapper(
                    method,
                    GetGenericArgumentsOrNull(method, methodArguments, methodReturnType));
            }

            private static Type[] GetGenericArgumentsOrNull(
                MethodInfo method,
                Expression[] methodArguments,
                Type methodReturnType)
            {
                if (!method.IsGenericMethod)
                {
                    return null;
                }

                var genericParameterTypes = method.GetGenericArguments();

                var methodParameters = method
                    .GetParameters()
                    .ProjectToArray((p, i) => new { Index = i, Parameter = p });

                var genericArguments = new Type[genericParameterTypes.Length];

                for (var i = 0; i < genericParameterTypes.Length; i++)
                {
                    var genericParameterType = genericParameterTypes[i];

                    if (genericParameterType == method.ReturnType)
                    {
                        genericArguments[i] = methodReturnType;
                        continue;
                    }

                    var matchingMethodParameter = methodParameters
                        .FirstOrDefault(p => p.Parameter.ParameterType == genericParameterType);

                    if (matchingMethodParameter == null)
                    {
                        return null;
                    }

                    var matchingMethodArgument = methodArguments
                        .ElementAtOrDefault(matchingMethodParameter.Index);

                    if (matchingMethodArgument == null)
                    {
                        return null;
                    }

                    genericArguments[i] = matchingMethodArgument.Type;
                }

                return genericArguments;
            }

            private class MissingMethod : IMethod
            {
                public MissingMethod(string name)
                {
                    Name = name;
                }

                public string Name { get; }

                public bool IsGenericMethod => false;

                public bool IsExtensionMethod => false;

                public IMethod GetGenericMethodDefinition() => null;

                public Type[] GetGenericArguments() => Enumerable<Type>.EmptyArray;

                public ParameterInfo[] GetParameters() => Enumerable<ParameterInfo>.EmptyArray;

                public Type ReturnType => typeof(void);
            }
        }
    }
}