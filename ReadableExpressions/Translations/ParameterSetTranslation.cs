namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
    using NetStandardPolyfills;
    using Reflection;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class ParameterSetTranslation : ITranslatable
    {
        private const int _splitArgumentsThreshold = 3;
        private const string _openAndCloseParentheses = "()";

        private readonly TranslationSettings _settings;
        private readonly IList<CodeBlockTranslation> _parameterTranslations;
        private readonly bool _hasSingleMultiStatementLambdaParameter;
        private ParenthesesMode _parenthesesMode;

        public ParameterSetTranslation(ITranslation parameter, ITranslationContext context)
        {
            _settings = context.Settings;
            _parameterTranslations = new[] { new CodeBlockTranslation(parameter, context) };
            TranslationSize = parameter.TranslationSize + _openAndCloseParentheses.Length;
            FormattingSize = parameter.FormattingSize;
            Count = 1;
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
            int count,
            ITranslationContext context)
        {
            _settings = context.Settings;
            _parenthesesMode = ParenthesesMode.Auto;

            if (count == 0)
            {
                _parameterTranslations = Enumerable<CodeBlockTranslation>.EmptyArray;
                TranslationSize = _openAndCloseParentheses.Length;
                return;
            }

            var methodProvided = method != null;

            if (methodProvided && method.IsExtensionMethod)
            {
                parameters = parameters.Skip(1);
                --count;
            }

            Count = count;

            IParameter[] methodParameters;

            if (methodProvided)
            {
                methodParameters = method.GetParameters();
                parameters = GetAllParameters(parameters, methodParameters);
            }
            else
            {
                methodParameters = null;
            }

            var hasSingleParameter = Count == 1;
            var singleParameterIsMultiLineLambda = false;
            var showParameterTypeNames = context.Settings.ShowLambdaParamTypes;
            var translationSize = 0;
            var formattingSize = 0;

            _parameterTranslations = parameters
                .Project((p, index) =>
                {
                    ITranslation translation;

                    if (CanBeConvertedToMethodGroup(p, out var lambdaBodyMethodCall))
                    {
                        translation = new MethodGroupTranslation(
                            Lambda,
                            MethodCallTranslation.GetSubjectTranslation(lambdaBodyMethodCall, context),
                            lambdaBodyMethodCall.Method,
                            context);

                        goto CreateCodeBlock;
                    }

                    if (methodProvided)
                    {
                        var parameterIndex = index;

                        if (Count != count)
                        {
                            // If a parameter is a params array then index will increase
                            // past parameterCount, so adjust here:
                            parameterIndex -= Count - count;
                        }

                        // ReSharper disable once PossibleNullReferenceException
                        translation = GetParameterTranslation(p, methodParameters[parameterIndex], context);
                        goto CreateCodeBlock;
                    }

                    translation = context.GetTranslationFor(p);

                    if (showParameterTypeNames &&
                        (translation is IParameterTranslation parameterTranslation))
                    {
                        parameterTranslation.WithTypeNames(context);
                        WithParentheses();
                    }

                    CreateCodeBlock:
                    translationSize += translation.TranslationSize;
                    formattingSize += translation.FormattingSize;

                    // TODO: Only use code blocks where useful:
                    var parameterCodeBlock = new CodeBlockTranslation(translation, context).WithoutTermination();

                    if (hasSingleParameter && parameterCodeBlock.IsMultiStatementLambda(context))
                    {
                        singleParameterIsMultiLineLambda = true;
                        parameterCodeBlock.WithSingleLamdaParameterFormatting();
                    }

                    return parameterCodeBlock;
                })
                .ToArray();

            _hasSingleMultiStatementLambdaParameter = singleParameterIsMultiLineLambda;
            TranslationSize = translationSize + (Count * ", ".Length) + 4;
            FormattingSize = formattingSize;
        }

        private IEnumerable<Expression> GetAllParameters(
            IEnumerable<Expression> parameters,
            IList<IParameter> methodParameters)
        {
            var i = 0;

            foreach (var parameter in parameters)
            {
                // params arrays are always the last parameter - if it's
                // not a NewArrayExpression it's a single Expression which
                // returns an Array, and doesn't need to be deconstructed:
                if ((i == (methodParameters.Count - 1)) &&
                     methodParameters[i].IsParamsArray &&
                     parameter is NewArrayExpression paramsArray)
                {
                    if (paramsArray.Expressions.Count > 0)
                    {
                        foreach (var paramsArrayValue in paramsArray.Expressions)
                        {
                            yield return paramsArrayValue;
                            ++Count;
                        }
                    }

                    --Count;
                    continue;
                }

                yield return parameter;
                ++i;
            }
        }

        private static ITranslation GetParameterTranslation(
            Expression parameter,
            IParameter info,
            ITranslationContext context)
        {
            if (info.IsOut)
            {
                return new OutParameterTranslation(parameter, context);
            }

            if (info.Type.IsByRef)
            {
                return new RefParameterTranslation(parameter, context);
            }

            return context.GetTranslationFor(parameter);
        }

        private static bool CanBeConvertedToMethodGroup(Expression argument, out MethodCallExpression lambdaBodyMethodCall)
        {
            if (argument.NodeType != Lambda)
            {
                lambdaBodyMethodCall = null;
                return false;
            }

            var argumentLambda = (LambdaExpression)argument;

            if (argumentLambda.Body.NodeType != Call)
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

        public int TranslationSize { get; private set; }

        public int FormattingSize { get; }

        public int Count { get; set; }

        public bool None => Count == 0;

        public ITranslation this[int parameterIndex] => _parameterTranslations[parameterIndex];

        public ParameterSetTranslation WithParentheses()
        {
            _parenthesesMode = ParenthesesMode.Always;
            return this;
        }

        public ParameterSetTranslation WithoutParentheses()
        {
            _parenthesesMode = ParenthesesMode.Never;
            TranslationSize -= 2;
            return this;
        }

        public ParameterSetTranslation WithoutTypeNames(ITranslationContext context)
        {
            var parameters = _parameterTranslations
                .Filter(p => p.NodeType == Parameter)
                .Project(p => p.AsParameterTranslation())
                .Filter(p => p != null);

            foreach (var parameter in parameters)
            {
                parameter.WithoutTypeNames(context);
            }

            return this;
        }

        public int GetIndentSize()
        {
            switch (Count)
            {
                case 0:
                    return 0;

                case 1:
                    return _parameterTranslations[0].GetIndentSize();
            }

            var indentSize = 0;
            var writeParametersOnNewLines = WriteParametersOnNewLines();
            var indentLength = _settings.IndentLength;

            for (var i = 0; ;)
            {
                var parameter = _parameterTranslations[i];
                var parameterIndentSize = parameter.GetIndentSize();

                if (writeParametersOnNewLines)
                {
                    parameterIndentSize += parameter.GetLineCount() * indentLength;
                }

                indentSize += parameterIndentSize;

                ++i;

                if (i == Count)
                {
                    return indentSize;
                }
            }
        }

        public int GetLineCount()
        {
            switch (Count)
            {
                case 0:
                    return 1;

                case 1:
                    return _parameterTranslations[0].GetLineCount();
            }

            var lineCount = 1;
            var writeParametersOnNewLines = WriteParametersOnNewLines();

            for (var i = 0; ;)
            {
                var parameterLineCount = _parameterTranslations[i].GetLineCount();

                if (parameterLineCount > 1)
                {
                    lineCount += parameterLineCount - 1;
                }

                if (writeParametersOnNewLines)
                {
                    lineCount += 1;
                }

                ++i;

                if (i == Count)
                {
                    return lineCount;
                }
            }
        }

        public void WriteTo(TranslationWriter writer)
        {
            switch (Count)
            {
                case 0:
                    if (_parenthesesMode != ParenthesesMode.Never)
                    {
                        writer.WriteToTranslation(_openAndCloseParentheses);
                    }

                    return;

                case 1 when (_parenthesesMode != ParenthesesMode.Always):
                    _parameterTranslations[0].WriteTo(writer);
                    return;
            }

            if (_parenthesesMode != ParenthesesMode.Never)
            {
                writer.WriteToTranslation('(');
            }

            var writeParametersOnNewLines = WriteParametersOnNewLines();

            if (writeParametersOnNewLines)
            {
                writer.WriteNewLineToTranslation();
                writer.Indent();
            }

            for (var i = 0; ;)
            {
                var parameterTranslation = _parameterTranslations[i];

                if (writeParametersOnNewLines && (i == 0) && parameterTranslation.IsMultiStatement)
                {
                    parameterTranslation.WithoutStartingNewLine();
                }

                parameterTranslation.WriteTo(writer);
                ++i;

                if (i == Count)
                {
                    break;
                }

                if (writeParametersOnNewLines)
                {
                    writer.WriteToTranslation(',');

                    if (!_parameterTranslations[i].IsMultiStatement)
                    {
                        writer.WriteNewLineToTranslation();
                    }

                    continue;
                }

                writer.WriteToTranslation(", ");
            }

            if (_parenthesesMode != ParenthesesMode.Never)
            {
                writer.WriteToTranslation(')');
            }

            if (writeParametersOnNewLines)
            {
                writer.Unindent();
            }
        }

        private bool WriteParametersOnNewLines()
        {
            if (_hasSingleMultiStatementLambdaParameter)
            {
                return false;
            }

            return (Count > _splitArgumentsThreshold) || this.ExceedsLengthThreshold();
        }

        private enum ParenthesesMode
        {
            Auto,
            Always,
            Never
        }

        private class OutParameterTranslation : ITranslation
        {
            private const string _out = "out ";
            private const string _var = "var ";
            private readonly ITranslation _parameterTranslation;
            private readonly ITranslation _typeNameTranslation;
            private readonly bool _declareParameterInline;

            public OutParameterTranslation(Expression parameter, ITranslationContext context)
            {
                _parameterTranslation = context.GetTranslationFor(parameter);
                TranslationSize = _parameterTranslation.TranslationSize + _out.Length;
                FormattingSize = _parameterTranslation.FormattingSize + context.GetKeywordFormattingSize();

                if ((parameter.NodeType == Parameter) &&
                     context.Settings.DeclareOutParamsInline &&
                     context.ShouldBeDeclaredInline((ParameterExpression)parameter))
                {
                    _declareParameterInline = true;

                    if (context.Settings.UseImplicitTypeNames)
                    {
                        TranslationSize += _var.Length;
                        FormattingSize += context.GetKeywordFormattingSize();
                        return;
                    }

                    _typeNameTranslation = context.GetTranslationFor(parameter.Type);
                    TranslationSize += _typeNameTranslation.TranslationSize + 1;
                    FormattingSize += _typeNameTranslation.FormattingSize;
                }
            }

            public ExpressionType NodeType => _parameterTranslation.NodeType;

            public Type Type => _parameterTranslation.Type;

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize()
            {
                var indentSize = _parameterTranslation.GetIndentSize();

                if (_typeNameTranslation != null)
                {
                    indentSize += _typeNameTranslation.GetIndentSize();
                }

                return indentSize;
            }

            public int GetLineCount()
            {
                var parameterLineCount = _parameterTranslation.GetLineCount();

                if (_declareParameterInline && _typeNameTranslation != null)
                {
                    parameterLineCount += _typeNameTranslation.GetLineCount();
                }

                return parameterLineCount;
            }

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteKeywordToTranslation(_out);

                if (_declareParameterInline)
                {
                    if (_typeNameTranslation != null)
                    {
                        _typeNameTranslation.WriteTo(writer);
                        writer.WriteSpaceToTranslation();
                    }
                    else
                    {
                        writer.WriteKeywordToTranslation(_var);
                    }
                }

                _parameterTranslation.WriteTo(writer);
            }
        }

        private class RefParameterTranslation : ITranslation
        {
            private const string _ref = "ref ";
            private readonly ITranslation _parameterTranslation;

            public RefParameterTranslation(Expression parameter, ITranslationContext context)
            {
                _parameterTranslation = context.GetTranslationFor(parameter);
                TranslationSize = _parameterTranslation.TranslationSize + _ref.Length;
                FormattingSize = _parameterTranslation.FormattingSize + context.GetKeywordFormattingSize();
            }

            public ExpressionType NodeType => _parameterTranslation.NodeType;

            public Type Type => _parameterTranslation.Type;

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize() => _parameterTranslation.GetIndentSize();

            public int GetLineCount() => _parameterTranslation.GetLineCount();

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteKeywordToTranslation(_ref);
                _parameterTranslation.WriteTo(writer);
            }
        }
    }
}