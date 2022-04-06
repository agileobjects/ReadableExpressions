﻿namespace AgileObjects.ReadableExpressions.Translations
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
    using Reflection;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    /// <summary>
    /// An <see cref="ITranslatable"/> which translates a set of Expressions representing parameters.
    /// </summary>
    public class ParameterSetTranslation : ITranslatable
    {
        private const int _splitArgumentsThreshold = 3;
        private const string _openAndCloseParentheses = "()";

        private readonly TranslationSettings _settings;
        private readonly IList<ITranslation> _parameterTranslations;
        private ExpressionType _singleParameterType;
        private ParenthesesMode _parenthesesMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSetTranslation"/> class with a single
        /// <see cref="IParameter"/>.
        /// </summary>
        /// <param name="parameter">The single <see cref="IParameter"/> to translate.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> describing the  Expression translation's context.
        /// </param>
        public ParameterSetTranslation(
            ITranslation parameter,
            ITranslationContext context)
        {
            var parameterTranslation = GetParameterTranslation(
                parameter,
                context,
                hasSingleParameter: true);

            _settings = context.Settings;
            _parameterTranslations = new[] { parameterTranslation };
            TranslationSize = parameter.TranslationSize + _openAndCloseParentheses.Length;
            FormattingSize = parameter.FormattingSize;
            Count = 1;
        }

        private ParameterSetTranslation(
            IMethodBase method,
            IEnumerable<Expression> parameters,
            bool showParameterTypeNames,
            int count,
            ITranslationContext context)
        {
            _settings = context.Settings;
            _parenthesesMode = ParenthesesMode.Auto;

            if (count == 0)
            {
                _parameterTranslations = Enumerable<ITranslation>.EmptyArray;
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

            IList<IParameter> methodParameters;

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
            var translationSize = 0;
            var formattingSize = 0;

            _parameterTranslations = parameters
                .Project((p, index) =>
                {
                    ITranslation translation;

                    if (context.Analysis.CanBeConvertedToMethodGroup(p, out var lambdaBodyMethodCall))
                    {
                        translation = new MethodGroupTranslation(
                            Lambda,
                            lambdaBodyMethodCall.GetSubjectTranslation(context),
                            lambdaBodyMethodCall.Method,
                            context);

                        goto FinaliseParameterTranslation;
                    }

                    if (methodProvided)
                    {
                        var parameterIndex = index;

                        if (Count != count)
                        {
                            // If a parameter is a params array then index will increase
                            // past parameterCount, so adjust:
                            parameterIndex -= Count - count;
                        }

                        // ReSharper disable once PossibleNullReferenceException
                        translation = GetParameterTranslation(p, methodParameters[parameterIndex], context);
                    }
                    else
                    {
                        translation = context.GetTranslationFor(p);
                    }

                    if (showParameterTypeNames &&
                       (translation is IParameterTranslation parameterTranslation))
                    {
                        parameterTranslation.WithTypeNames(context);
                        WithParentheses();
                    }

                FinaliseParameterTranslation:
                    translationSize += translation.TranslationSize;
                    formattingSize += translation.FormattingSize;

                    return GetParameterTranslation(translation, context, hasSingleParameter);
                })
                .ToList();

            TranslationSize = translationSize + (Count * ", ".Length) + 4;
            FormattingSize = formattingSize;
        }

        #region Setup

        private IEnumerable<Expression> GetAllParameters(
            IEnumerable<Expression> parameters,
            IList<IParameter> methodParameters)
        {
            var i = 0;
            var finalParameterIndex = methodParameters.Count - 1;

            foreach (var parameter in parameters)
            {
                // params arrays are always the last parameter - if it's
                // not a NewArrayExpression it's a single Expression which
                // returns an Array, and doesn't need to be deconstructed:
                if ((i == finalParameterIndex) &&
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
            if (context.Analysis.ShouldBeDiscarded(parameter))
            {
                return info.IsOut
                    ? new DiscardedOutParameterTranslation(parameter, context)
                    : new DiscardedParameterTranslation(parameter, context);
            }

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

        private ITranslation GetParameterTranslation(
            ITranslation translation,
            ITranslationContext context,
            bool hasSingleParameter)
        {
            switch (translation.NodeType)
            {
                case Default:
                case Parameter:
                    return translation;
            }

            var parameterCodeBlock = new CodeBlockTranslation(translation, context).WithoutTermination();

            if (hasSingleParameter)
            {
                if (parameterCodeBlock.NodeType == Block)
                {
                    _singleParameterType = Block;
                }
                else if (parameterCodeBlock.IsMultiStatementLambda)
                {
                    _singleParameterType = Lambda;
                    parameterCodeBlock.WithSingleLamdaParameterFormatting();
                }
            }

            return parameterCodeBlock;
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a <see cref="ParameterSetTranslation"/> for the given
        /// <paramref name="lambda"/>'s parameters, using the given <paramref name="context"/>.
        /// </summary>
        /// <param name="lambda">
        /// The LambdaExpression the parameters for which to create the
        /// <see cref="ParameterSetTranslation"/>.
        /// </param>
        /// <param name="context">The <see cref="ITranslationContext"/> to use.</param>
        /// <returns>
        /// A <see cref="ParameterSetTranslation"/> for the given <paramref name="lambda"/>'s
        /// parameters.
        /// </returns>
        public static ParameterSetTranslation For(
            LambdaExpression lambda,
            ITranslationContext context)
        {
            var lambdaMethod = new LambdaExpressionMethodAdapter(lambda);

            var showParameterTypeNames =
                context.Settings.ShowLambdaParamTypes ||
                lambdaMethod.GetParameters().Any(p => p.IsOut || p.IsRef);

            return For(
                lambdaMethod,
                lambda.Parameters,
                showParameterTypeNames,
                context);
        }

        /// <summary>
        /// Creates a <see cref="ParameterSetTranslation"/> for the given
        /// <paramref name="parameters"/>, using the given <paramref name="context"/>.
        /// </summary>
        /// <typeparam name="TParameterExpression">
        /// The type of Expression representing each parameter to translate.
        /// </typeparam>
        /// <param name="parameters">
        /// The parameters for which to create the <see cref="ParameterSetTranslation"/>.
        /// </param>
        /// <param name="context">The <see cref="ITranslationContext"/> to use.</param>
        /// <returns>
        /// A <see cref="ParameterSetTranslation"/> for the given <paramref name="parameters"/>.
        /// </returns>
        public static ParameterSetTranslation For<TParameterExpression>(
            ICollection<TParameterExpression> parameters,
            ITranslationContext context)
            where TParameterExpression : Expression
        {
            return For(method: null, parameters, context);
        }

        /// <summary>
        /// Creates a <see cref="ParameterSetTranslation"/> for the given
        /// <paramref name="method"/> and <paramref name="parameters"/>, using the given
        /// <paramref name="context"/>.
        /// </summary>
        /// <typeparam name="TParameterExpression">
        /// The type of Expression representing each parameter to translate.
        /// </typeparam>
        /// <param name="method">
        /// The <see cref="IMethod"/> for which to create the <see cref="ParameterSetTranslation"/>.
        /// </param>
        /// <param name="parameters">
        /// The parameters for which to create the <see cref="ParameterSetTranslation"/>.
        /// </param>
        /// <param name="context">The <see cref="ITranslationContext"/> to use.</param>
        /// <returns>
        /// A <see cref="ParameterSetTranslation"/> for the given <paramref name="method"/> and
        /// <paramref name="parameters"/>.
        /// </returns>
        public static ParameterSetTranslation For<TParameterExpression>(
            IMethod method,
            ICollection<TParameterExpression> parameters,
            ITranslationContext context)
            where TParameterExpression : Expression
        {
            return For(method, parameters, showParameterTypeNames: false, context);
        }

        private static ParameterSetTranslation For<TParameterExpression>(
            IMethodBase method,
            ICollection<TParameterExpression> parameters,
            bool showParameterTypeNames,
            ITranslationContext context)
            where TParameterExpression : Expression
        {
            return new(
                method,
#if NET35
                parameters.Cast<Expression>().ToArray(),
#else
                parameters,
#endif
                showParameterTypeNames,
                parameters.Count,
                context);
        }

        #endregion

        /// <inheritdoc />
        public int TranslationSize { get; private set; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <summary>
        /// Gets the number of parameters described by this <see cref="ParameterSetTranslation"/>.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ParameterSetTranslation"/> describes an
        /// empty set of parameters.
        /// </summary>
        public bool None => Count == 0;

        /// <summary>
        /// Gets the <see cref="ITranslation"/> at the given <paramref name="parameterIndex"/> in
        /// this <see cref="ParameterSetTranslation"/>.
        /// </summary>
        /// <param name="parameterIndex">The index for which to retrieve the <see cref="ITranslation"/>.</param>
        /// <returns>
        /// The <see cref="ITranslation"/> at the given <paramref name="parameterIndex"/> in this
        /// <see cref="ParameterSetTranslation"/>.
        /// </returns>
        public ITranslation this[int parameterIndex] => _parameterTranslations[parameterIndex];

        /// <summary>
        /// Enforces parentheses in the output of this <see cref="ParameterSetTranslation"/>.
        /// </summary>
        /// <returns>This <see cref="ParameterSetTranslation"/>, to support a fluent API.</returns>
        public ParameterSetTranslation WithParentheses()
        {
            _parenthesesMode = ParenthesesMode.Always;
            return this;
        }

        /// <summary>
        /// Enforces no parentheses in the output of this <see cref="ParameterSetTranslation"/>.
        /// </summary>
        /// <returns>This <see cref="ParameterSetTranslation"/>, to support a fluent API.</returns>
        public ParameterSetTranslation WithoutParentheses()
        {
            _parenthesesMode = ParenthesesMode.Never;
            TranslationSize -= 2;
            return this;
        }

        internal ParameterSetTranslation WithoutTypeNames(ITranslationContext context)
        {
            var parameters = _parameterTranslations
                .Filter(p => p.NodeType == Parameter)
                .OfType<IParameterTranslation>();

            foreach (var parameter in parameters)
            {
                parameter.WithoutTypeNames(context);
            }

            return this;
        }

        internal void WithoutNullArguments(ITranslationContext context)
        {
            for (var i = 0; i < Count; ++i)
            {
                var parameter = _parameterTranslations[i];

                if (parameter.NodeType != Default)
                {
                    continue;
                }

                if (parameter is INullKeywordTranslation)
                {
                    _parameterTranslations[i] = new CodeBlockTranslation(
                        new DefaultOperatorTranslation(parameter.Type, context),
                        context);
                }
            }
        }

        /// <inheritdoc />
        public int GetIndentSize()
        {
            var indentParameters = IndentParameters && WriteParametersOnNewLines();

            switch (Count)
            {
                case 0:
                    return 0;

                case 1 when !indentParameters:
                    return _parameterTranslations[0].GetIndentSize();
            }

            var indentSize = 0;
            var indentLength = _settings.IndentLength;

            for (var i = 0; ;)
            {
                var parameter = _parameterTranslations[i];
                var parameterIndentSize = parameter.GetIndentSize();

                if (indentParameters)
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
            var indent = IndentParameters;

            if (writeParametersOnNewLines)
            {
                writer.WriteNewLineToTranslation();

                if (indent)
                {
                    writer.Indent();
                }
            }

            for (var i = 0; ;)
            {
                var parameterTranslation = _parameterTranslations[i];
                var parameterCodeBlock = parameterTranslation as CodeBlockTranslation;

                if (writeParametersOnNewLines && (i == 0) && parameterCodeBlock?.IsMultiStatement == true)
                {
                    parameterCodeBlock.WithoutStartingNewLine();
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

                    parameterCodeBlock = _parameterTranslations[i] as CodeBlockTranslation;

                    if (parameterCodeBlock?.IsMultiStatement != true)
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

            if (indent && writeParametersOnNewLines)
            {
                writer.Unindent();
            }
        }

        private bool WriteParametersOnNewLines()
        {
            if (_singleParameterType == Lambda)
            {
                return false;
            }

            return (Count > _splitArgumentsThreshold) || this.ExceedsLengthThreshold();
        }

        private bool IndentParameters => _singleParameterType != Block;

        private enum ParenthesesMode
        {
            Auto,
            Always,
            Never
        }

        private abstract class KeywordParameterTranslationBase : IParameterTranslation
        {
            private readonly string _keyword;
            private readonly ITranslation _parameterTranslation;
            private readonly IParameterTranslation _wrappedParameterTranslation;
            private readonly Func<Type, ITranslation> _typeNameTranslationFactory;
            private int _translationSize;
            private int _formattingSize;
            private ITranslation _typeNameTranslation;

            protected KeywordParameterTranslationBase(
                string keyword,
                Expression parameter,
                ITranslationContext context)
            {
                _keyword = keyword;
                _parameterTranslation = context.GetTranslationFor(parameter);
                _wrappedParameterTranslation = _parameterTranslation as IParameterTranslation;
                _typeNameTranslationFactory = context.GetTranslationFor;
                _translationSize = _parameterTranslation.TranslationSize + keyword.Length;
                _formattingSize = _parameterTranslation.FormattingSize + context.GetKeywordFormattingSize();
            }

            public ExpressionType NodeType => _parameterTranslation.NodeType;

            public virtual Type Type => _parameterTranslation.Type;

            public virtual string Name => _wrappedParameterTranslation?.Name;

            public virtual int TranslationSize => _translationSize;

            public virtual int FormattingSize => _formattingSize;

            protected ITranslation TypeNameTranslation
            {
                get => _typeNameTranslation ??= _typeNameTranslationFactory.Invoke(Type);
                private set => _typeNameTranslation = value;
            }

            protected bool WriteTypeName { get; private set; }

            public virtual int GetIndentSize() => _parameterTranslation.GetIndentSize();

            public virtual int GetLineCount() => _parameterTranslation.GetLineCount();

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteKeywordToTranslation(_keyword);
                WriteTo(writer, _parameterTranslation);
            }

            protected virtual void WriteTo(
                TranslationWriter writer,
                ITranslation parameterTranslation)
            {
            }

            public ITranslation WithTypeNames(ITranslationContext context)
            {
                if (_wrappedParameterTranslation == null)
                {
                    return null;
                }

                WriteTypeName = true;
                SubtractParameterTranslationSizes();

                TypeNameTranslation = _wrappedParameterTranslation
                    .WithTypeNames(context);

                AddParameterTranslationSizes();
                return TypeNameTranslation;
            }

            public void WithoutTypeNames(ITranslationContext context)
            {
                if (_wrappedParameterTranslation == null)
                {
                    return;
                }

                WriteTypeName = false;
                SubtractParameterTranslationSizes();
                _wrappedParameterTranslation.WithoutTypeNames(context);
                AddParameterTranslationSizes();
            }

            private void SubtractParameterTranslationSizes()
            {
                _translationSize -= _wrappedParameterTranslation.TranslationSize;
                _formattingSize -= _wrappedParameterTranslation.FormattingSize;
            }

            private void AddParameterTranslationSizes()
            {
                _translationSize += _wrappedParameterTranslation.TranslationSize;
                _formattingSize += _wrappedParameterTranslation.FormattingSize;
            }
        }

        private class DiscardedParameterTranslation : KeywordParameterTranslationBase
        {
            private const string _discard = "_";

            public DiscardedParameterTranslation(
                Expression parameter,
                ITranslationContext context) :
                this(keyword: string.Empty, parameter, context)
            {
            }

            protected DiscardedParameterTranslation(
                string keyword,
                Expression parameter,
                ITranslationContext context) :
                base(keyword, parameter, context)
            {
            }

            public override Type Type => typeof(void);

            public override string Name => _discard;

            public override int TranslationSize
                => base.TranslationSize + _discard.Length;

            protected override void WriteTo(
                TranslationWriter writer,
                ITranslation parameterTranslation)
            {
                if (WriteTypeName)
                {
                    TypeNameTranslation.WriteTo(writer);
                    writer.WriteSpaceToTranslation();
                }

                writer.WriteToTranslation(_discard);
            }
        }

        private class DiscardedOutParameterTranslation : DiscardedParameterTranslation
        {
            private const string _out = "out ";
            private readonly int _formattingSize;

            public DiscardedOutParameterTranslation(
                Expression parameter,
                ITranslationContext context) : base(_out, parameter, context)
            {
                _formattingSize = context.GetKeywordFormattingSize();
            }

            public override int TranslationSize
                => base.TranslationSize + _out.Length;

            public override int FormattingSize
                => base.FormattingSize + _formattingSize;
        }

        private class OutParameterTranslation : KeywordParameterTranslationBase
        {
            private const string _out = "out ";
            private const string _var = "var ";
            private readonly bool _declareParameterInline;
            private readonly bool _useImplicitTypeName;
            private readonly int _translationSize;
            private readonly int _formattingSize;

            public OutParameterTranslation(
                Expression parameter,
                ITranslationContext context) : base(_out, parameter, context)
            {
                if (DoNotDeclareInline(parameter, context))
                {
                    return;
                }

                _declareParameterInline = true;

                if (!context.Settings.UseImplicitTypeNames)
                {
                    return;
                }

                _useImplicitTypeName = true;
                _translationSize += _var.Length;
                _formattingSize += context.GetKeywordFormattingSize();
            }

            #region Setup

            private static bool DoNotDeclareInline(
                Expression parameter,
                ITranslationContext context)
            {
                return !context.Analysis
                    .ShouldBeDeclaredInOutputParameterUse(parameter);
            }

            #endregion

            public override int TranslationSize
                => base.TranslationSize + _translationSize;

            public override int FormattingSize
                => base.FormattingSize + _formattingSize;

            protected override void WriteTo(
                TranslationWriter writer,
                ITranslation parameterTranslation)
            {
                if (_declareParameterInline)
                {
                    if (_useImplicitTypeName)
                    {
                        writer.WriteKeywordToTranslation(_var);
                    }
                    else
                    {
                        TypeNameTranslation.WriteTo(writer);
                        writer.WriteSpaceToTranslation();
                    }
                }

                parameterTranslation.WriteTo(writer);
            }
        }

        private class RefParameterTranslation : KeywordParameterTranslationBase
        {
            private const string _ref = "ref ";

            public RefParameterTranslation(
                Expression parameter,
                ITranslationContext context) : base(_ref, parameter, context)
            {
            }

            protected override void WriteTo(TranslationWriter writer,
                ITranslation parameterTranslation)
            {
                parameterTranslation.WriteTo(writer);
            }
        }
    }
}