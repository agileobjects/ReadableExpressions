namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.ObjectModel;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Api;
    using Extensions;
    using Translations.Reflection;

    /// <summary>
    /// Represents a method in a class in a piece of source code.
    /// </summary>
    public class MethodExpression : Expression, IMethodNamingContext
    {
        private readonly ClassExpression _parent;
        private readonly TranslationSettings _settings;

        private MethodExpression(
            ClassExpression parent,
            LambdaExpression definition,
            TranslationSettings settings)
        {
            _parent = parent;
            Definition = definition;
            _settings = settings;

            IParameter[] parameters;

            var parameterCount = definition.Parameters.Count;

            if (parameterCount != 0)
            {
                var methodParameters = new MethodParameterExpression[parameterCount];
                parameters = new IParameter[parameterCount];

                for (var i = 0; i < parameterCount; i++)
                {
                    parameters[i] = methodParameters[i] =
                        new MethodParameterExpression(definition.Parameters[i]);
                }

                Parameters = methodParameters.ToReadOnlyCollection();
            }
            else
            {
                parameters = Enumerable<IParameter>.EmptyArray;
                Parameters = Enumerable<MethodParameterExpression>.EmptyReadOnlyCollection;
            }

            Method = new MethodExpressionMethod(this, parameters, settings);
        }

        #region Factory Methods

        internal static MethodExpression For(Expression expression, TranslationSettings settings)
            => For(null, expression, settings);

        internal static MethodExpression For(
            ClassExpression parent,
            Expression expression,
            TranslationSettings settings)
        {
            var definition = (expression.NodeType == ExpressionType.Lambda)
                ? (LambdaExpression) expression
                : expression.ToLambdaExpression();

            return For(parent, definition, settings);
        }

        internal static MethodExpression For(
            ClassExpression parent,
            LambdaExpression definition,
            TranslationSettings settings)
        {
            return new MethodExpression(parent, definition, settings);
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1002) indicating the type of this
        /// <see cref="MethodExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.Method;

        /// <summary>
        /// Gets the type of this <see cref="MethodExpression"/>, which is the return type of the
        /// LambdaExpression from which the method was created.
        /// </summary>
        public override Type Type => ReturnType;

        /// <summary>
        /// Visits this <see cref="MethodExpression"/>'s Body.
        /// </summary>
        /// <param name="visitor">The visitor with which to visit this <see cref="MethodExpression"/>.</param>
        /// <returns>This <see cref="MethodExpression"/>.</returns>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            foreach (var parameter in Parameters)
            {
                visitor.Visit(parameter);
            }

            visitor.Visit(Body);
            return this;
        }

        /// <summary>
        /// Gets the return type of this <see cref="MethodExpression"/>, which is the return type
        /// of the LambdaExpression from which the method was created.
        /// </summary>
        public Type ReturnType => Definition.ReturnType;

        /// <summary>
        /// Gets the <see cref="MethodParameterExpression"/>s describing the parameters of this
        /// <see cref="MethodExpression"/>.
        /// </summary>
        public ReadOnlyCollection<MethodParameterExpression> Parameters { get; }

        /// <summary>
        /// Gets the LambdaExpression describing the parameters and body of this
        /// <see cref="MethodExpression"/>.
        /// </summary>
        public LambdaExpression Definition { get; }

        /// <summary>
        /// Gets the Expression describing the body of this <see cref="MethodExpression"/>.
        /// </summary>
        public Expression Body => Definition.Body;

        internal IMethod Method { get; }

        #region IMethodNamingContext Members

        Type IMethodNamingContext.ReturnType => Type;

        string IMethodNamingContext.ReturnTypeName
            => Type.GetVariableNameInPascalCase(_settings);

        LambdaExpression IMethodNamingContext.MethodLambda => Definition;

        int IMethodNamingContext.Index => _parent?.Methods.IndexOf(this) ?? 0;

        #endregion

        private class MethodExpressionMethod : IMethod
        {
            private readonly MethodExpression _method;
            private readonly IParameter[] _parameters;
            private readonly TranslationSettings _settings;
            private string _name;

            public MethodExpressionMethod(
                MethodExpression method,
                IParameter[] parameters,
                TranslationSettings settings)
            {
                _method = method;
                _parameters = parameters;
                _settings = settings;
            }

            private ClassExpression Parent => _method._parent;

            public Type DeclaringType => null;

            public bool IsPublic => true;

            public bool IsProtectedInternal => false;

            public bool IsInternal => false;

            public bool IsProtected => false;

            public bool IsPrivate => false;

            public bool IsAbstract => false;

            public bool IsStatic => false;

            public bool IsVirtual => false;

            public string Name =>
                _name ??= _settings.MethodNameFactory.Invoke(Parent?.Parent, Parent, _method);

            public bool IsGenericMethod => false;

            public bool IsExtensionMethod => false;

            public Type ReturnType => _method.Type;

            public IMethod GetGenericMethodDefinition() => null;

            public Type[] GetGenericArguments()
                => Enumerable<Type>.EmptyArray;

            public IParameter[] GetParameters() => _parameters;
        }
    }
}