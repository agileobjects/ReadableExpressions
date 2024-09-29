namespace AgileObjects.ReadableExpressions.Translations;

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Extensions;
using Formatting;
using NetStandardPolyfills;
using Reflection;

internal static class DynamicTranslation
{
    public static INodeTranslation For(
        DynamicExpression dynamicExpression,
        ITranslationContext context)
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

        if (DynamicMethodCallTranslation.TryGetTranslation(args, context, out translation))
        {
            return translation;
        }

        return new FixedValueTranslation(
            ExpressionType.Dynamic,
            args.OperationDescription,
            TokenType.Default);
    }

    private class DynamicTranslationArgs
    {
        private readonly DynamicExpression _dynamicExpression;

        public DynamicTranslationArgs(
            DynamicExpression dynamicExpression,
            ITranslationContext context)
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
        private static readonly Regex _matcher = new(@"^GetMember (?<MemberName>[^\(]+)\(");

        public static bool TryGetTranslation(
            DynamicTranslationArgs args,
            out INodeTranslation translation)
        {
            if (!args.IsMatch(_matcher, out var match))
            {
                translation = null;
                return false;
            }

            translation = GetTranslation(args, match);
            return true;
        }

        public static INodeTranslation GetTranslation(
            DynamicTranslationArgs args,
            Match match)
        {
            var subject = args.Context.GetTranslationFor(args.FirstArgument);
            var memberName = match.Groups["MemberName"].Value;

            return new MemberAccessTranslation(subject, memberName, args.Context);
        }
    }

    private static class DynamicMemberWriteTranslation
    {
        private static readonly Regex _matcher = new(@"^SetMember (?<MemberName>[^\(]+)\(");

        public static bool TryGetTranslation(
            DynamicTranslationArgs args,
            out INodeTranslation translation)
        {
            if (!args.IsMatch(_matcher, out var match))
            {
                translation = null;
                return false;
            }

            var targetTranslation = DynamicMemberAccessTranslation
                .GetTranslation(args, match);

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
        private static readonly Regex _matcher = new(@"^Call (?<MethodName>[^\(]+)\(");

        public static bool TryGetTranslation(
            DynamicTranslationArgs args,
            ITranslationContext context,
            out INodeTranslation translation)
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
                args.ExpressionType,
                context);

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
            IList<Expression> methodArguments,
            Type methodReturnType,
            ITranslationContext context)
        {
            if (method == null)
            {
                return new MissingMethod(methodName);
            }

            return new ClrMethodWrapper(
                method,
                GetGenericArguments(method, methodArguments, methodReturnType),
                context.Settings);
        }

        private static Type[] GetGenericArguments(
            MethodInfo method,
            IList<Expression> methodArguments,
            Type methodReturnType)
        {
            if (!method.IsGenericMethod)
            {
                return Enumerable<Type>.EmptyArray;
            }

            var genericParameterTypes = method.GetGenericArguments();
            var genericParameterCount = genericParameterTypes.Length;
            var genericArguments = new Type[genericParameterCount];
            var methodParameters = method.GetParameters();
            var methodParameterCount = methodParameters.Length;
            var methodArgumentCount = methodArguments.Count;

            for (var i = 0; i < genericParameterCount; i++)
            {
                var genericParameterType = genericParameterTypes[i];

                if (genericParameterType == method.ReturnType)
                {
                    genericArguments[i] = methodReturnType;
                    continue;
                }

                if (methodArgumentCount == 0)
                {
                    return Enumerable<Type>.EmptyArray;
                }

                for (var j = 0; j < methodParameterCount; ++j)
                {
                    var parameter = methodParameters[j];

                    if (parameter.ParameterType != genericParameterType)
                    {
                        continue;
                    }

                    if (j < methodArgumentCount)
                    {
                        genericArguments[i] = methodArguments[j].Type;
                        break;
                    }

                    return Enumerable<Type>.EmptyArray;
                }
            }

            return genericArguments;
        }

        private class MissingMethod : IMethod
        {
            public MissingMethod(string name)
            {
                Name = name;
            }

            public IType DeclaringType => null;

            public bool IsPublic => false;

            public bool IsInternal => false;

            public bool IsProtectedInternal => false;

            public bool IsProtected => false;

            public bool IsPrivateProtected => false;

            public bool IsPrivate => false;

            public bool IsAbstract => false;

            public bool IsStatic => false;

            public bool IsVirtual => false;

            public bool IsOverride => this.IsOverride();

            public string Name { get; }

            public bool IsGenericMethod => false;

            public bool IsExtensionMethod => false;

            public bool HasBody => false;

            public IMethod GetGenericMethodDefinition() => null;

            public ReadOnlyCollection<IGenericParameter> GetGenericArguments() => 
                Enumerable<IGenericParameter>.EmptyReadOnlyCollection;

            public ReadOnlyCollection<IParameter> GetParameters() => 
                Enumerable<IParameter>.EmptyReadOnlyCollection;

            public IType Type => ReturnType;

            public IType ReturnType => ClrTypeWrapper.Void;
        }
    }
}