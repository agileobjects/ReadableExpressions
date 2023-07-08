namespace AgileObjects.ReadableExpressions.Translations;

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
/// An <see cref="ITranslation"/> which translates a set of Expressions representing parameters.
/// </summary>
public class ParameterSetTranslation : IPotentialParenthesizedTranslation
{
    private const int _splitArgumentsThreshold = 3;
    private const string _openAndCloseParentheses = "()";

    private readonly IList<INodeTranslation> _parameterTranslations;
    private bool _indentParameters;
    private bool _isSingleLambdaParameter;
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
        INodeTranslation parameter,
        ITranslationContext context) : this()
    {
        var parameterTranslation = GetParameterTranslation(
            parameter,
            context,
            hasSingleParameter: true);

        _parameterTranslations = new[] { parameterTranslation };
        Count = 1;
    }

    private ParameterSetTranslation(
        IMethodBase method,
        IEnumerable<Expression> parameters,
        bool showParameterTypeNames,
        int count,
        ITranslationContext context) : this()
    {
        _parenthesesMode = ParenthesesMode.Auto;

        if (count == 0)
        {
            _parameterTranslations = Enumerable<INodeTranslation>.EmptyArray;
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

        _parameterTranslations = parameters
            .Project((p, index) =>
            {
                INodeTranslation translation;

                if (context.Analysis.CanBeConvertedToMethodGroup(p, out var lambdaBodyMethodCall))
                {
                    translation = new MethodGroupTranslation(
                        Lambda,
                        lambdaBodyMethodCall.GetSubjectTranslation(context),
                        lambdaBodyMethodCall.Method);

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
                    translation is IParameterTranslation parameterTranslation)
                {
                    parameterTranslation.WithTypeNames(context);
                    WithParentheses();
                }

            FinaliseParameterTranslation:
                return GetParameterTranslation(translation, context, hasSingleParameter);
            })
            .ToList();
    }

    private ParameterSetTranslation() => _indentParameters = true;

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
            if (i == finalParameterIndex &&
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

    private static INodeTranslation GetParameterTranslation(
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

    private INodeTranslation GetParameterTranslation(
        INodeTranslation translation,
        ITranslationContext context,
        bool hasSingleParameter)
    {
        switch (translation.NodeType)
        {
            case Default:
            case Parameter:
                return translation;
        }

        var parameterCodeBlock = new CodeBlockTranslation(translation, context)
            .WithoutTermination();

        if (!hasSingleParameter || !parameterCodeBlock.IsMultiStatement)
        {
            return parameterCodeBlock;
        }

        _indentParameters = false;

        if (!parameterCodeBlock.IsMultiStatementLambda)
        {
            return parameterCodeBlock;
        }

        _isSingleLambdaParameter = true;
        return parameterCodeBlock.WithSingleLamdaParameterFormatting();
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
    public int TranslationLength
    {
        get
        {
            var translationSize = _parameterTranslations
                .TotalTranslationLength(separator: ", ");

            return _parenthesesMode != ParenthesesMode.Never
                ? translationSize + "()".Length
                : translationSize;
        }
    }

    /// <summary>
    /// Gets the number of parameters described by this <see cref="ParameterSetTranslation"/>.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="ParameterSetTranslation"/> describes an
    /// empty set of parameters.
    /// </summary>
    public bool None => Count == 0;

    bool IPotentialParenthesizedTranslation.Parenthesize
        => _parenthesesMode != ParenthesesMode.Never;

    /// <summary>
    /// Gets the <see cref="INodeTranslation"/> at the given <paramref name="parameterIndex"/> in
    /// this <see cref="ParameterSetTranslation"/>.
    /// </summary>
    /// <param name="parameterIndex">The index for which to retrieve the <see cref="INodeTranslation"/>.</param>
    /// <returns>
    /// The <see cref="INodeTranslation"/> at the given <paramref name="parameterIndex"/> in this
    /// <see cref="ParameterSetTranslation"/>.
    /// </returns>
    public INodeTranslation this[int parameterIndex] => _parameterTranslations[parameterIndex];

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

            if (parameter is INullKeywordTranslation nullTranslation)
            {
                _parameterTranslations[i] = new CodeBlockTranslation(
                    new DefaultOperatorTranslation(nullTranslation.NullType, context),
                    context);
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

            case 1 when _parenthesesMode != ParenthesesMode.Always:
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

            if (_indentParameters)
            {
                writer.Indent();
            }
        }

        for (var i = 0; ;)
        {
            var parameterTranslation = _parameterTranslations[i];
            var parameterCodeBlock = parameterTranslation as CodeBlockTranslation;

            if (writeParametersOnNewLines && i == 0 && parameterCodeBlock?.IsMultiStatement == true)
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

        if (_indentParameters && writeParametersOnNewLines)
        {
            writer.Unindent();
        }
    }

    private bool WriteParametersOnNewLines()
    {
        if (_isSingleLambdaParameter)
        {
            return false;
        }

        return Count > _splitArgumentsThreshold || this.WrapLine();
    }

    private enum ParenthesesMode
    {
        Auto,
        Always,
        Never
    }

    private abstract class KeywordParameterTranslationBase : IParameterTranslation
    {
        private readonly string _keyword;
        private readonly Type _parameterType;
        private readonly INodeTranslation _parameterTranslation;
        private readonly IParameterTranslation _wrappedParameterTranslation;
        private readonly Func<Type, INodeTranslation> _typeNameTranslationFactory;
        private int _translationSize;
        private INodeTranslation _typeNameTranslation;

        protected KeywordParameterTranslationBase(
            string keyword,
            Expression parameter,
            ITranslationContext context)
        {
            _keyword = keyword;
            _parameterType = parameter.Type;
            _parameterTranslation = context.GetTranslationFor(parameter);
            _wrappedParameterTranslation = _parameterTranslation as IParameterTranslation;
            _typeNameTranslationFactory = context.GetTranslationFor;
            _translationSize = _parameterTranslation.TranslationLength + keyword.Length;
        }

        public ExpressionType NodeType => _parameterTranslation.NodeType;

        public virtual string Name => _wrappedParameterTranslation?.Name;

        public virtual int TranslationLength => _translationSize;

        protected INodeTranslation TypeNameTranslation
        {
            get => _typeNameTranslation ??= _typeNameTranslationFactory.Invoke(_parameterType);
            private set => _typeNameTranslation = value;
        }

        protected bool WriteTypeName { get; private set; }

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_keyword);
            WriteTo(writer, _parameterTranslation);
        }

        protected virtual void WriteTo(
            TranslationWriter writer,
            INodeTranslation parameterTranslation)
        {
        }

        public INodeTranslation WithTypeNames(ITranslationContext context)
        {
            if (_wrappedParameterTranslation == null)
            {
                return null;
            }

            WriteTypeName = true;
            SubtractParameterTranslationSize();

            TypeNameTranslation = _wrappedParameterTranslation
                .WithTypeNames(context);

            AddParameterTranslationSize();
            return TypeNameTranslation;
        }

        public void WithoutTypeNames(ITranslationContext context)
        {
            if (_wrappedParameterTranslation == null)
            {
                return;
            }

            WriteTypeName = false;
            SubtractParameterTranslationSize();
            _wrappedParameterTranslation.WithoutTypeNames(context);
            AddParameterTranslationSize();
        }

        private void SubtractParameterTranslationSize()
            => _translationSize -= _wrappedParameterTranslation.TranslationLength;

        private void AddParameterTranslationSize()
            => _translationSize += _wrappedParameterTranslation.TranslationLength;
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

        public override string Name => _discard;

        public override int TranslationLength
            => base.TranslationLength + _discard.Length;

        protected override void WriteTo(
            TranslationWriter writer,
            INodeTranslation parameterTranslation)
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

        public DiscardedOutParameterTranslation(
            Expression parameter,
            ITranslationContext context) : base(_out, parameter, context)
        {
        }

        public override int TranslationLength
            => base.TranslationLength + _out.Length;
    }

    private class OutParameterTranslation : KeywordParameterTranslationBase
    {
        private const string _out = "out ";
        private const string _var = "var ";
        private readonly bool _declareParameterInline;
        private readonly bool _useImplicitTypeName;
        private readonly int _translationSize;

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

        public override int TranslationLength
            => base.TranslationLength + _translationSize;

        protected override void WriteTo(
            TranslationWriter writer,
            INodeTranslation parameterTranslation)
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
            INodeTranslation parameterTranslation)
        {
            parameterTranslation.WriteTo(writer);
        }
    }
}