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
    using System.Reflection;
    using Extensions;
    using Formatting;
    using NetStandardPolyfills;
    using Reflection;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    /// <summary>
    /// Provides methods for creating <see cref="ITranslation"/>s for different types of methods call.
    /// </summary>
    public static class MethodCallTranslation
    {
        /// <summary>
        /// Creates an <see cref="ITranslation"/> for the given <paramref name="invocation"/>.
        /// </summary>
        /// <param name="invocation">The InvocationExpression for which to create the <see cref="ITranslation"/>.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> in which the translation is being performed.
        /// </param>
        /// <returns>An <see cref="ITranslation"/> for the given <paramref name="invocation"/>.</returns>
        public static ITranslation For(InvocationExpression invocation, ITranslationContext context)
        {
            var invocationMethod = invocation.Expression.Type.GetPublicInstanceMethod("Invoke");

            var method = new ClrMethodWrapper(invocationMethod, context);
            var parameters = ParameterSetTranslation.For(method, invocation.Arguments, context).WithParentheses();
            var subject = context.GetTranslationFor(invocation.Expression);

            if (subject.NodeType == Lambda)
            {
                subject = subject.WithParentheses();
            }

            return new StandardMethodCallTranslation(Invoke, subject, method, parameters, context);
        }

        /// <summary>
        /// Creates an <see cref="ITranslation"/> for the given <paramref name="methodCall"/>.
        /// </summary>
        /// <param name="methodCall">The MethodCallExpression for which to create the <see cref="ITranslation"/>.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> in which the translation is being performed.
        /// </param>
        /// <returns>An <see cref="ITranslation"/> for the given <paramref name="methodCall"/>.</returns>
        public static ITranslation For(MethodCallExpression methodCall, ITranslationContext context)
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

            if (IsStringConcatCall(methodCall))
            {
                return new StringConcatenationTranslation(Call, methodCall.Arguments, context);
            }

            var method = new ClrMethodWrapper(methodCall.Method, context);
            var parameters = ParameterSetTranslation.For(method, methodCall.Arguments, context);

            if (methodCall.Method.IsImplicitOperator())
            {
                return new CodeBlockTranslation(parameters[0], context).WithNodeType(Call);
            }

            var subject = methodCall.GetSubjectTranslation(context);

            if (IsIndexedPropertyAccess(methodCall))
            {
                return new IndexAccessTranslation(subject, parameters, methodCall.Type);
            }

            parameters = parameters.WithParentheses();

            if (methodCall.Method.IsExplicitOperator())
            {
                return CastTranslation.ForExplicitOperator(
                    parameters[0],
                    context.GetTranslationFor(methodCall.Method.ReturnType));
            }

            var methodCallTranslation = new StandardMethodCallTranslation(Call, subject, method, parameters, context);

            if (context.IsPartOfMethodCallChain(methodCall))
            {
                methodCallTranslation.AsPartOfMethodCallChain();
            }

            return methodCallTranslation;
        }

        private static bool IsStringConcatCall(MethodCallExpression methodCall)
        {
            return methodCall.Method.IsStatic &&
                  (methodCall.Method.DeclaringType == typeof(string)) &&
                  (methodCall.Method.Name == nameof(string.Concat));
        }

        /// <summary>
        /// Get an <see cref="ITranslation"/> for the subject of this <paramref name="methodCall"/>.
        /// </summary>
        /// <param name="methodCall">
        /// The MethodCallExpression for which to retrieve the subject <see cref="ITranslation"/>.
        /// </param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> in which the translation is being performed.
        /// </param>
        /// <returns></returns>
        public static ITranslation GetSubjectTranslation(
            this MethodCallExpression methodCall,
            ITranslationContext context)
        {
            return context.GetTranslationFor(methodCall.GetSubject()) ??
                   context.GetTranslationFor(methodCall.Method.DeclaringType);
        }

        private static bool IsIndexedPropertyAccess(MethodCallExpression methodCall)
        {
            var property = methodCall
                .Object?
                .Type
                .GetPublicInstanceProperties()
                .FirstOrDefault(p => p.IsIndexer() && p.GetAccessors().Contains(methodCall.Method));

            return property?.GetIndexParameters().Any() == true;
        }

        /// <summary>
        /// Creates an <see cref="ITranslation"/> for the given <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The <see cref="IMethod"/> for which to create the <see cref="ITranslation"/>.</param>
        /// <param name="methodParameters">Expressions describing the <paramref name="method"/>'s parameters.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> in which the translation is being performed.
        /// </param>
        /// <returns>An <see cref="ITranslation"/> for the given <paramref name="method"/>.</returns>
        public static ITranslation For<TParameterExpression>(
            IMethod method,
            ICollection<TParameterExpression> methodParameters,
            ITranslationContext context)
            where TParameterExpression : Expression
        {
            var subject = method.IsStatic && !method.IsExtensionMethod
                ? context.GetTranslationFor(method.DeclaringType)
                : (ITranslation)new FixedValueTranslation(
                    MemberAccess,
                    "this",
                    typeof(object),
                    TokenType.Keyword,
                    context);

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
        /// Creates an <see cref="ITranslation"/> for the given custom type <paramref name="castMethod"/>.
        /// </summary>
        /// <param name="typeNameTranslation">
        /// An <see cref="ITranslation"/> for the type to which the cast is being performed.
        /// </param>
        /// <param name="castMethod">The <see cref="IMethod"/> for which to create the <see cref="ITranslation"/>.</param>
        /// <param name="castValue">An <see cref="ITranslation"/> for the value being cast.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> in which the translation is being performed.
        /// </param>
        /// <returns>An <see cref="ITranslation"/> for the given <paramref name="castMethod"/>.</returns>
        public static ITranslation ForCustomMethodCast(
            ITranslation typeNameTranslation,
            IMethod castMethod,
            ITranslation castValue,
            ITranslationContext context)
        {
            return new StandardMethodCallTranslation(
                Call,
                typeNameTranslation,
                castMethod,
                new ParameterSetTranslation(castValue, context).WithParentheses(),
                context);
        }

        /// <summary>
        /// Creates an <see cref="ITranslation"/> for the given dynamic <paramref name="method"/>.
        /// </summary>
        /// <param name="subjectTranslation">
        /// An <see cref="ITranslation"/> for the object on which the dynamic method call is performed.
        /// </param>
        /// <param name="method">The <see cref="IMethod"/> for which to create the <see cref="ITranslation"/>.</param>
        /// <param name="arguments">Expressions describing the <paramref name="method"/>'s arguments.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> in which the translation is being performed.
        /// </param>
        /// <returns>An <see cref="ITranslation"/> for the given <paramref name="method"/>.</returns>
        public static ITranslation ForDynamicMethodCall(
            ITranslation subjectTranslation,
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
                ITranslationContext context)
                : base(
                    GetSubjectOrNull(getterCall.Object, property, context),
                    property.Name,
                    property.PropertyType,
                    context)
            {
            }
        }

        private class PropertySetterTranslation : AssignmentTranslation
        {
            public PropertySetterTranslation(
                MethodCallExpression setterCall,
                ITranslation getterTranslation,
                ITranslationContext context)
                : base(Assign, getterTranslation, setterCall.Arguments.First(), context)
            {
            }
        }

        private class StandardMethodCallTranslation : ITranslation
        {
            private readonly ITranslationContext _context;
            private readonly ITranslation _subjectTranslation;
            private readonly MethodInvocationTranslation _methodInvocationTranslation;
            private bool _isPartOfMethodCallChain;

            public StandardMethodCallTranslation(
                ExpressionType nodeType,
                ITranslation subjectTranslation,
                IMethod method,
                ParameterSetTranslation parameters,
                ITranslationContext context)
            {
                _context = context;
                NodeType = nodeType;
                _subjectTranslation = subjectTranslation;
                _methodInvocationTranslation = new MethodInvocationTranslation(method, parameters, context);

                TranslationSize =
                    _subjectTranslation.TranslationSize +
                    ".".Length +
                    _methodInvocationTranslation.TranslationSize;

                FormattingSize =
                    _subjectTranslation.FormattingSize +
                    _methodInvocationTranslation.FormattingSize;
            }

            public ExpressionType NodeType { get; }

            public Type Type => _methodInvocationTranslation.Type;

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public void AsPartOfMethodCallChain() => _isPartOfMethodCallChain = true;

            public int GetIndentSize()
            {
                var indentSize = _subjectTranslation.GetIndentSize();

                indentSize += _isPartOfMethodCallChain
                    ? _methodInvocationTranslation.GetLineCount() * _context.Settings.IndentLength
                    : _methodInvocationTranslation.GetLineCount();

                return indentSize;
            }

            public int GetLineCount()
            {
                var lineCount =
                    _subjectTranslation.GetLineCount() +
                    _methodInvocationTranslation.GetLineCount();

                if (_isPartOfMethodCallChain)
                {
                    ++lineCount;
                }

                return lineCount;
            }

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

        private class MethodInvocationTranslation : ITranslatable
        {
            private readonly IMethod _method;
            private readonly ParameterSetTranslation _parameters;
            private readonly ITranslatable[] _explicitGenericArguments;
            private readonly int _explicitGenericArgumentCount;

            public MethodInvocationTranslation(
                IMethod method,
                ParameterSetTranslation parameters,
                ITranslationContext context)
            {
                _method = method;
                _parameters = parameters;
                _explicitGenericArguments = GetRequiredExplicitGenericArguments(context, out var translationsSize);
                _explicitGenericArgumentCount = _explicitGenericArguments.Length;

                if (method.IsGenericMethod && _explicitGenericArgumentCount == 0)
                {
                    parameters.WithoutNullArguments(context);
                }

                TranslationSize = method.Name.Length + translationsSize + parameters.TranslationSize;

                FormattingSize =
                    _explicitGenericArgumentCount * context.GetTypeNameFormattingSize() +
                     parameters.FormattingSize;
            }

            private ITranslatable[] GetRequiredExplicitGenericArguments(
                ITranslationContext context,
                out int translationsSize)
            {
                var requiredGenericArguments = _method
                    .GetRequiredExplicitGenericArguments(context.Settings);

                if (!requiredGenericArguments.Any())
                {
                    translationsSize = 0;
                    return Enumerable<ITranslatable>.EmptyArray;
                }

                var argumentTranslationsSize = 0;

                var arguments = requiredGenericArguments
                    .Project<IGenericParameter, ITranslatable>(argument =>
                    {
                        var argumentTypeTranslation = context.GetTranslationFor(argument);
                        argumentTranslationsSize += argumentTypeTranslation.TranslationSize + 2;

                        return argumentTypeTranslation;
                    })
                    .Filter(argument => argument != null)
                    .ToArray();

                translationsSize = argumentTranslationsSize;

                return (translationsSize != 0) ? arguments : Enumerable<ITranslatable>.EmptyArray;
            }

            public Type Type => _method.ReturnType.AsType();

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize() => _parameters.GetIndentSize();

            public int GetLineCount() => _parameters.GetLineCount();

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
}
