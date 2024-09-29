namespace AgileObjects.ReadableExpressions.Translations;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
using Formatting;
using NetStandardPolyfills;
using Reflection;
using static ExpressionType;
using static StringConcatenationTranslation;

/// <summary>
/// Provides methods for creating <see cref="INodeTranslation"/>s for different types of methods call.
/// </summary>
public static class MethodCallTranslation
{
    /// <summary>
    /// Creates an <see cref="INodeTranslation"/> for the given <paramref name="invocation"/>.
    /// </summary>
    /// <param name="invocation">The InvocationExpression for which to create the <see cref="INodeTranslation"/>.</param>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> in which the translation is being performed.
    /// </param>
    /// <returns>An <see cref="INodeTranslation"/> for the given <paramref name="invocation"/>.</returns>
    public static INodeTranslation For(
        InvocationExpression invocation,
        ITranslationContext context)
    {
        var subjectExpression = invocation.Expression;
        var invocationMethod = subjectExpression.Type.GetPublicInstanceMethod("Invoke");

        var method = new ClrMethodWrapper(invocationMethod, context);
        var parameters = ParameterSetTranslation.For(method, invocation.Arguments, context).WithParentheses();
        var subject = context.GetTranslationFor(subjectExpression);

        if (subjectExpression.NodeType == Lambda)
        {
            subject = subject.WithParentheses();
        }

        return new StandardMethodCallTranslation(Invoke, subject, method, parameters, context);
    }

    /// <summary>
    /// Creates an <see cref="INodeTranslation"/> for the given <paramref name="methodCall"/>.
    /// </summary>
    /// <param name="methodCall">The MethodCallExpression for which to create the <see cref="INodeTranslation"/>.</param>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> in which the translation is being performed.
    /// </param>
    /// <returns>An <see cref="INodeTranslation"/> for the given <paramref name="methodCall"/>.</returns>
    public static INodeTranslation For(
        MethodCallExpression methodCall,
        ITranslationContext context)
    {
        if (methodCall.Method.IsAccessor(out var property) && !property.IsIndexer())
        {
            var getterTranslation = new PropertyGetterTranslation(methodCall, property, context);

            if (methodCall.HasReturnType())
            {
                return getterTranslation;
            }

            return new PropertySetterTranslation(methodCall, getterTranslation, context);
        }

        if (TryCreateForConcatCall(methodCall, context, out var concatTranslation))
        {
            return concatTranslation;
        }

        var method = new ClrMethodWrapper(methodCall.Method, context);
        var parameters = ParameterSetTranslation.For(method, methodCall.Arguments, context);

        if (methodCall.Method.IsImplicitOperator())
        {
            return new CodeBlockTranslation(parameters[0], context).WithNodeType(Call);
        }

        if (IsDefaultIndexedPropertyAccess(methodCall))
        {
            return new IndexAccessTranslation(methodCall, parameters, context);
        }

        parameters = parameters.WithParentheses();

        if (methodCall.Method.IsExplicitOperator())
        {
            return CastTranslation.ForExplicitOperator(
                parameters[0],
                context.GetTranslationFor(methodCall.Method.ReturnType));
        }

        var isCapturedCallResultValue = TryGetCapturedCallResultValue(
            methodCall,
            context,
            out var callResultTranslation);

        if (isCapturedCallResultValue)
        {
            return callResultTranslation;
        }

        return new StandardMethodCallTranslation(
            Call,
            methodCall,
            method,
            parameters,
            context);
    }

    /// <summary>
    /// Get an <see cref="INodeTranslation"/> for the subject of this <paramref name="methodCall"/>.
    /// </summary>
    /// <param name="methodCall">
    /// The MethodCallExpression for which to retrieve the subject <see cref="INodeTranslation"/>.
    /// </param>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> in which the translation is being performed.
    /// </param>
    /// <returns></returns>
    public static INodeTranslation GetSubjectTranslation(
        this MethodCallExpression methodCall,
        ITranslationContext context)
    {
        return context.GetTranslationFor(methodCall.GetSubject()) ??
               context.GetTranslationFor(methodCall.Method.DeclaringType);
    }

    private static bool IsDefaultIndexedPropertyAccess(MethodCallExpression methodCall)
    {
        return methodCall.Method.IsHideBySig &&
               methodCall.Method.GetProperty()?.GetIndexParameters().Any() == true;
    }

    private static bool TryGetCapturedCallResultValue(
        MethodCallExpression methodCall,
        ITranslationContext context,
        out INodeTranslation resultTranslation)
    {
        var subject = methodCall.GetSubject();

        if (subject.IsCapture(out var capturedSubject) &&
            methodCall.Method.TryGetValue(capturedSubject.Object, out var callResult) &&
            ConstantTranslation.TryCreateValueTranslation(
                callResult,
                methodCall.Type,
                context,
                out resultTranslation))
        {
            return true;
        }

        resultTranslation = null;
        return false;
    }

    /// <summary>
    /// Creates an <see cref="INodeTranslation"/> for the given <paramref name="method"/>.
    /// </summary>
    /// <param name="method">The <see cref="IMethod"/> for which to create the <see cref="INodeTranslation"/>.</param>
    /// <param name="methodParameters">Expressions describing the <paramref name="method"/>'s parameters.</param>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> in which the translation is being performed.
    /// </param>
    /// <returns>An <see cref="INodeTranslation"/> for the given <paramref name="method"/>.</returns>
    public static INodeTranslation For<TParameterExpression>(
        IMethod method,
        ICollection<TParameterExpression> methodParameters,
        ITranslationContext context)
        where TParameterExpression : Expression
    {
        var subject = method.IsStatic && !method.IsExtensionMethod
            ? context.GetTranslationFor(method.DeclaringType)
            : (INodeTranslation)new FixedValueTranslation(
                MemberAccess,
                "this",
                TokenType.Keyword);

        var parameters = ParameterSetTranslation
            .For(method, methodParameters, context)
            .WithParentheses();

        return new StandardMethodCallTranslation(
            Call,
            subject,
            method,
            parameters,
            context);
    }

    /// <summary>
    /// Creates an <see cref="INodeTranslation"/> for the given custom type <paramref name="castMethod"/>.
    /// </summary>
    /// <param name="castMethod">The <see cref="IMethod"/> for which to create the <see cref="INodeTranslation"/>.</param>
    /// <param name="castValue">An <see cref="INodeTranslation"/> for the value being cast.</param>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> in which the translation is being performed.
    /// </param>
    /// <returns>An <see cref="INodeTranslation"/> for the given <paramref name="castMethod"/>.</returns>
    public static INodeTranslation ForCustomMethodCast(
        IMethod castMethod,
        INodeTranslation castValue,
        ITranslationContext context)
    {
        var castMethodSubjectTranslation =
            context.GetTranslationFor(castMethod.DeclaringType);

        return new StandardMethodCallTranslation(
            Call,
            castMethodSubjectTranslation,
            castMethod,
            new ParameterSetTranslation(castValue, context).WithParentheses(),
            context);
    }

    /// <summary>
    /// Creates an <see cref="INodeTranslation"/> for the given dynamic <paramref name="method"/>.
    /// </summary>
    /// <param name="subjectTranslation">
    /// An <see cref="INodeTranslation"/> for the object on which the dynamic method call is performed.
    /// </param>
    /// <param name="method">The <see cref="IMethod"/> for which to create the <see cref="INodeTranslation"/>.</param>
    /// <param name="arguments">Expressions describing the <paramref name="method"/>'s arguments.</param>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> in which the translation is being performed.
    /// </param>
    /// <returns>An <see cref="INodeTranslation"/> for the given <paramref name="method"/>.</returns>
    public static INodeTranslation ForDynamicMethodCall(
        INodeTranslation subjectTranslation,
        IMethod method,
        ICollection<Expression> arguments,
        ITranslationContext context)
    {
        return new StandardMethodCallTranslation(
            Dynamic,
            subjectTranslation,
            method,
            ParameterSetTranslation.For(arguments, context).WithParentheses(),
            context);
    }

    private class PropertyGetterTranslation : MemberAccessTranslation
    {
        public PropertyGetterTranslation(
            MethodCallExpression getterCall,
            PropertyInfo property,
            ITranslationContext context) :
            base(
                GetSubjectTranslationOrNull(getterCall.Object, property, context),
                property.Name,
                context)
        {
        }
    }

    private class PropertySetterTranslation : AssignmentTranslation
    {
        public PropertySetterTranslation(
            MethodCallExpression setterCall,
            INodeTranslation getterTranslation,
            ITranslationContext context) :
            base(Assign, getterTranslation, setterCall.Arguments.First(), context)
        {
        }
    }

    private class StandardMethodCallTranslation : INodeTranslation
    {
        private readonly ITranslationContext _context;
        private readonly INodeTranslation _subjectTranslation;
        private readonly MethodInvocationTranslation _methodInvocationTranslation;
        private readonly bool _isPartOfMethodCallChain;

        public StandardMethodCallTranslation(
            ExpressionType nodeType,
            MethodCallExpression methodCall,
            IMethod method,
            ParameterSetTranslation parameters,
            ITranslationContext context) :
            this(
                nodeType,
                methodCall.GetSubjectTranslation(context),
                method,
                parameters,
                context)
        {
            if (context.Analysis.IsPartOfMethodCallChain(methodCall))
            {
                _isPartOfMethodCallChain = true;
            }
        }

        public StandardMethodCallTranslation(
            ExpressionType nodeType,
            INodeTranslation subjectTranslation,
            IMethod method,
            ParameterSetTranslation parameters,
            ITranslationContext context)
        {
            _context = context;
            NodeType = nodeType;
            _subjectTranslation = subjectTranslation;
            _methodInvocationTranslation = new(method, parameters, context);
        }

        public ExpressionType NodeType { get; }

        public int TranslationLength =>
            _subjectTranslation.TranslationLength +
            ".".Length +
            _methodInvocationTranslation.TranslationLength;

        public void WriteTo(TranslationWriter writer)
        {
            _subjectTranslation.WriteInParenthesesIfRequired(writer, _context);

            if (_isPartOfMethodCallChain)
            {
                writer.WriteNewLineToTranslation();
                writer.Indent();
            }

            writer.WriteDotToTranslation();
            _methodInvocationTranslation.WriteTo(writer);

            if (_isPartOfMethodCallChain)
            {
                writer.Unindent();
            }
        }
    }

    private class MethodInvocationTranslation : ITranslation
    {
        private readonly IMethod _method;
        private readonly ParameterSetTranslation _parameters;
        private readonly ITranslation[] _explicitGenericArguments;
        private readonly int _explicitGenericArgumentCount;

        public MethodInvocationTranslation(
            IMethod method,
            ParameterSetTranslation parameters,
            ITranslationContext context)
        {
            _method = method;
            _parameters = parameters;
            _explicitGenericArguments = GetRequiredExplicitGenericArguments(context);
            _explicitGenericArgumentCount = _explicitGenericArguments.Length;

            if (method.IsGenericMethod && _explicitGenericArgumentCount == 0)
            {
                parameters.WithoutNullArguments(context);
            }
        }

        private ITranslation[] GetRequiredExplicitGenericArguments(
            ITranslationContext context)
        {
            var requiredGenericArguments = _method
                .GetRequiredExplicitGenericArguments(context.Settings);

            if (requiredGenericArguments.None())
            {
                return Enumerable<ITranslation>.EmptyArray;
            }

            var arguments = requiredGenericArguments
                .Project<IGenericParameter, ITranslation>(context.GetTranslationFor)
                .Filter(argument => argument != null)
                .ToArray();

            return arguments;
        }

        public int TranslationLength =>
            _method.Name.Length +
            _explicitGenericArguments.TotalTranslationLength(separator: ", ") +
            _parameters.TranslationLength;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteToTranslation(_method.Name, TokenType.MethodName);
            WriteGenericArgumentNamesIfNecessary(writer);
            _parameters.WriteTo(writer);
        }

        private void WriteGenericArgumentNamesIfNecessary(TranslationWriter writer)
        {
            if (_explicitGenericArgumentCount == 0)
            {
                return;
            }

            writer.WriteToTranslation('<');

            for (var i = 0; ;)
            {
                _explicitGenericArguments[i].WriteTo(writer);

                ++i;

                if (i == _explicitGenericArgumentCount)
                {
                    break;
                }

                writer.WriteToTranslation(", ");
            }

            writer.WriteToTranslation('>');
        }
    }
}