namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    internal class ParameterSet : FormattableExpressionBase
    {
        private readonly IEnumerable<Func<string, string>> _parameterModifiers;
        private readonly IEnumerable<Expression> _arguments;
        private readonly TranslationContext _context;
        private readonly IEnumerable<Func<Expression, string>> _argumentTranslators;

        public ParameterSet(
            IMethodInfo method,
            IEnumerable<Expression> arguments,
            TranslationContext context)
        {
            _parameterModifiers = GetParameterModifers(method);
            _arguments = arguments;
            _context = context;

            _argumentTranslators = GetArgumentTranslators(
                method,
                arguments,
                context.Translate);
        }

        private static IEnumerable<Func<string, string>> GetParameterModifers(IMethodInfo method)
        {
            if (method == null)
            {
                return Enumerable.Empty<Func<string, string>>();
            }

            return method
                .GetParameters()
                .Select(GetParameterModifier)
                .ToArray();
        }

        private static Func<string, string> GetParameterModifier(ParameterInfo parameter)
        {
            if (parameter.IsOut)
            {
                return p => "out " + p;
            }

            if (parameter.ParameterType.IsByRef)
            {
                return p => "ref " + p;
            }

            if (parameter.IsParamsArray())
            {
                return FormatParamsArray;
            }

            return default(Func<string, string>);
        }

        private static string FormatParamsArray(string array)
        {
            var arrayValuesStart = array.IndexOf('{') + 1;
            var arrayValuesEnd = array.LastIndexOf('}');
            var arrayValues = array.Substring(arrayValuesStart, arrayValuesEnd - arrayValuesStart).Trim();

            return arrayValues;
        }

        private IEnumerable<Func<Expression, string>> GetArgumentTranslators(
            IMethodInfo method,
            IEnumerable<Expression> arguments,
            Func<Expression, string> defaultArgumentTranslator)
        {
            if (method == null)
            {
                return arguments.Select(argument => defaultArgumentTranslator).ToArray();
            }

            var parameters = method.GetParameters();

            if (method.IsExtensionMethod)
            {
                parameters = parameters.Skip(1).ToArray();
            }

            return arguments
                .Select((argument, i) =>
                {
                    var parameter = parameters.ElementAtOrDefault(i);

                    if (IsNotFuncType(parameter, method))
                    {
                        return defaultArgumentTranslator;
                    }

                    return CanBeConvertedToMethodGroup(argument)
                        ? MethodGroupTranslator
                        : defaultArgumentTranslator;
                })
                .ToArray();
        }

        private static bool IsNotFuncType(ParameterInfo parameter, IMethodInfo method)
        {
            if (parameter == null)
            {
                return true;
            }

            if (parameter.ParameterType.GetBaseType() == typeof(MulticastDelegate))
            {
                return false;
            }

            var parameterType = parameter.ParameterType;

            if (parameterType.FullName == null)
            {
                if (!method.IsGenericMethod)
                {
                    return true;
                }

                parameterType = method.GetGenericArgumentFor(parameter.ParameterType);
            }

            return !(parameterType.FullName.StartsWith("System.Action", StringComparison.Ordinal) ||
                parameterType.FullName.StartsWith("System.Func", StringComparison.Ordinal)) ||
                // ReSharper disable once PossibleUnintendedReferenceComparison
                (parameter.ParameterType.GetAssembly() != typeof(Action).GetAssembly());
        }

        private static bool CanBeConvertedToMethodGroup(Expression argument)
        {
            if (argument.NodeType != ExpressionType.Lambda)
            {
                return false;
            }

            var argumentLambda = (LambdaExpression)argument;

            if (argumentLambda.Body.NodeType != ExpressionType.Call)
            {
                return false;
            }

            var lambdaBodyMethodCall = (MethodCallExpression)argumentLambda.Body;
            var lambdaBodyMethodCallArguments = lambdaBodyMethodCall.Arguments.ToArray();

            if (lambdaBodyMethodCall.Method.IsExtensionMethod())
            {
                lambdaBodyMethodCallArguments = lambdaBodyMethodCallArguments.Skip(1).ToArray();
            }

            if (lambdaBodyMethodCallArguments.Length != argumentLambda.Parameters.Count)
            {
                return false;
            }

            var i = 0;

            var allArgumentTypesMatch = argumentLambda
                .Parameters
                .All(lambdaParameter => lambdaBodyMethodCallArguments[i++] == lambdaParameter);

            return allArgumentTypesMatch;
        }

        private Func<Expression, string> MethodGroupTranslator
        {
            get
            {
                return argument =>
                {
                    var methodCall = (MethodCallExpression)((LambdaExpression)argument).Body;

                    var subject = MethodCallExpressionTranslator
                        .GetMethodCallSubject(methodCall, _context);

                    return subject + "." + methodCall.Method.Name;
                };
            }
        }

        protected override bool SplitToMultipleLines(string translation)
        {
            return (_arguments.Count() > 3) || base.SplitToMultipleLines(translation);
        }

        protected override Func<string> SingleLineTranslationFactory => () => FormatParameters(", ");

        protected override Func<string> MultipleLineTranslationFactory
        {
            get
            {
                return () =>
                    Environment.NewLine +
                    FormatParameters("," + Environment.NewLine, a => a.Indented());
            }
        }

        private string FormatParameters(
            string separator,
            Func<string, string> extraFormatter = null)
        {
            if (extraFormatter == null)
            {
                extraFormatter = s => s;
            }

            return string.Join(
                separator,
                _arguments
                    .Select(TranslateArgument)
                    .Select(extraFormatter)
                    .Where(arg => arg != string.Empty));
        }

        private string TranslateArgument(Expression argument, int parameterIndex)
        {
            var argumentTranslator = _argumentTranslators.ElementAt(parameterIndex);
            var argumentString = argumentTranslator.Invoke(argument).Unterminated();
            var modifier = _parameterModifiers.ElementAtOrDefault(parameterIndex);

            return (modifier != null) ? modifier.Invoke(argumentString) : argumentString;
        }

        public string WithParenthesesIfNecessary()
        {
            return (_arguments.Count() == 1) ? WithoutParentheses() : WithParentheses();
        }

        public string WithoutParentheses()
        {
            return GetFormattedTranslation();
        }

        public string WithParentheses()
        {
            return _arguments.Any() ? GetFormattedTranslation().WithSurroundingParentheses() : "()";
        }
    }
}