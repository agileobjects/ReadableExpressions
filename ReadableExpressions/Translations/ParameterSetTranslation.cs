namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
    using NetStandardPolyfills;

    internal class ParameterSetTranslation : ITranslatable
    {
        private const int _splitArgumentsThreshold = 3;
        private const string _openAndCloseParentheses = "()";

        private readonly IList<CodeBlockTranslation> _parameterTranslations;
        private ParenthesesMode _parenthesesMode;

        public ParameterSetTranslation(ITranslation parameter)
        {
            _parameterTranslations = new[] { new CodeBlockTranslation(parameter) };
            EstimatedSize = parameter.EstimatedSize + _openAndCloseParentheses.Length;
            ParameterCount = 1;
        }

        public ParameterSetTranslation(IEnumerable<ParameterExpression> parameters, ITranslationContext context)
#if NET35
            : this(null, parameters.Cast<Expression>().ToArray(), context)
#else
            : this(null, parameters.ToArray(), context)
#endif
        {
        }

        public ParameterSetTranslation(ICollection<ParameterExpression> parameters, ITranslationContext context)
#if NET35
            : this(null, parameters.Cast<Expression>(), parameters.Count, context)
#else
            : this(null, parameters, parameters.Count, context)
#endif
        {
        }

        public ParameterSetTranslation(ICollection<Expression> parameters, ITranslationContext context)
            : this(null, parameters, parameters.Count, context)
        {
        }

        public ParameterSetTranslation(
            IMethod method,
            ICollection<Expression> parameters,
            ITranslationContext context)
            : this(method, parameters, parameters.Count, context)
        {
        }

        private ParameterSetTranslation(
            IMethod method,
            IEnumerable<Expression> parameters,
            int parameterCount,
            ITranslationContext context)
        {
            _parenthesesMode = ParenthesesMode.Auto;

            if (parameterCount == 0)
            {
                _parameterTranslations = Enumerable<CodeBlockTranslation>.EmptyArray;
                EstimatedSize = _openAndCloseParentheses.Length;
                return;
            }

            var methodProvided = method != null;

            if (methodProvided && method.IsExtensionMethod)
            {
                parameters = parameters.Skip(1);
                --parameterCount;
            }

            ParameterCount = parameterCount;

            ParameterInfo[] methodParameters;

            if (methodProvided)
            {
                methodParameters = method.GetParameters();
                parameters = GetAllParameters(parameters, methodParameters);
            }
            else
            {
                methodParameters = null;
            }

            var estimatedSize = 0;

            _parameterTranslations = parameters
                .Project((p, index) =>
                {
                    ITranslation translation;

                    if (CanBeConvertedToMethodGroup(p, out var lambdaBodyMethodCall))
                    {
                        translation = new MethodGroupTranslation(
                            ExpressionType.Lambda,
                            MethodCallTranslation.GetSubjectTranslation(lambdaBodyMethodCall, context),
                            lambdaBodyMethodCall.Method);

                        goto CreateCodeBlock;
                    }

                    if (methodProvided)
                    {
                        // If a parameter is a params array then index will increase
                        // past parameterCount, so adjust here:
                        var parameterIndex = (ParameterCount != parameterCount)
                            ? ParameterCount - parameterCount - index
                            : index;

                        // ReSharper disable once PossibleNullReferenceException
                        translation = GetParameterTranslation(p, methodParameters[parameterIndex], context);
                    }
                    else
                    {
                        translation = context.GetTranslationFor(p);
                    }

                    CreateCodeBlock:
                    estimatedSize += translation.EstimatedSize;

                    return new CodeBlockTranslation(translation).WithoutTermination();
                })
                .ToArray();

            EstimatedSize = estimatedSize + (ParameterCount * 2) + 4;
        }

        private IEnumerable<Expression> GetAllParameters(
            IEnumerable<Expression> parameters,
            IList<ParameterInfo> methodParameters)
        {
            var i = 0;

            foreach (var parameter in parameters)
            {
                // params arrays are always the last parameter:
                if ((i == (methodParameters.Count - 1)) &&
                     methodParameters[i].IsParamsArray())
                {
                    var paramsArray = (NewArrayExpression)parameter;

                    if (paramsArray.Expressions.Count > 0)
                    {
                        foreach (var paramsArrayValue in paramsArray.Expressions)
                        {
                            yield return paramsArrayValue;
                            ++ParameterCount;
                        }
                    }

                    --ParameterCount;
                    continue;
                }

                yield return parameter;
                ++i;
            }
        }

        private static ITranslation GetParameterTranslation(
            Expression parameter,
            ParameterInfo info,
            ITranslationContext context)
        {
            var translation = context.GetTranslationFor(parameter);

            if (info.IsOut)
            {
                return new TranslationWrapper(translation).WithPrefix("out ");
            }

            if (info.ParameterType.IsByRef)
            {
                return new TranslationWrapper(translation).WithPrefix("ref ");
            }

            return translation;
        }

        private static bool CanBeConvertedToMethodGroup(Expression argument, out MethodCallExpression lambdaBodyMethodCall)
        {
            if (argument.NodeType != ExpressionType.Lambda)
            {
                lambdaBodyMethodCall = null;
                return false;
            }

            var argumentLambda = (LambdaExpression)argument;

            if (argumentLambda.Body.NodeType != ExpressionType.Call)
            {
                lambdaBodyMethodCall = null;
                return false;
            }

            lambdaBodyMethodCall = (MethodCallExpression)argumentLambda.Body;

            IList<Expression> lambdaBodyMethodCallArguments = lambdaBodyMethodCall.Arguments;

            if (lambdaBodyMethodCall.Method.IsExtensionMethod())
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

        public int EstimatedSize { get; }

        private int ParameterCount { get; set; }

        public bool None => ParameterCount == 0;

        public ITranslation this[int parameterIndex] => _parameterTranslations[parameterIndex];

        public ParameterSetTranslation WithParentheses()
        {
            _parenthesesMode = ParenthesesMode.Always;
            return this;
        }

        public ParameterSetTranslation WithoutParentheses()
        {
            _parenthesesMode = ParenthesesMode.Never;
            return this;
        }

        public void WriteTo(TranslationBuffer buffer)
        {
            switch (ParameterCount)
            {
                case 0:
                    buffer.WriteToTranslation(_openAndCloseParentheses);
                    return;

                case 1 when (_parenthesesMode != ParenthesesMode.Always):
                    _parameterTranslations[0].WriteTo(buffer);
                    return;
            }

            if (_parenthesesMode != ParenthesesMode.Never)
            {
                buffer.WriteToTranslation('(');
            }

            var writeParametersOnNewLines = (ParameterCount > _splitArgumentsThreshold) || this.ExceedsLengthThreshold();

            if (writeParametersOnNewLines)
            {
                buffer.WriteNewLineToTranslation();
                buffer.Indent();
            }

            for (var i = 0; ;)
            {
                var parameterTranslation = _parameterTranslations[i];

                if (writeParametersOnNewLines && (i == 0) && parameterTranslation.IsMultiStatement)
                {
                    parameterTranslation.WithoutStartingNewLine();
                }

                parameterTranslation.WriteTo(buffer);

                if (++i == ParameterCount)
                {
                    break;
                }

                if (writeParametersOnNewLines)
                {
                    buffer.WriteToTranslation(',');

                    if (!_parameterTranslations[i].IsMultiStatement)
                    {
                        buffer.WriteNewLineToTranslation();
                    }

                    continue;
                }

                buffer.WriteToTranslation(", ");
            }

            if (_parenthesesMode != ParenthesesMode.Never)
            {
                buffer.WriteToTranslation(')');
            }

            if (writeParametersOnNewLines)
            {
                buffer.Unindent();
            }
        }

        private enum ParenthesesMode
        {
            Auto,
            Always,
            Never
        }
    }
}