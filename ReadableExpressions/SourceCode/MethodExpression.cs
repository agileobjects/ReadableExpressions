namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.ObjectModel;
    using Api;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Translations.Reflection;

    /// <summary>
    /// Represents a method in a class in a piece of source code.
    /// </summary>
    public class MethodExpression : Expression
    {
        private readonly ClassExpression _parent;

        private MethodExpression(
            ClassExpression parent,
            LambdaExpression body,
            TranslationSettings settings)
        {
            _parent = parent;
            IParameter[] parameters;

            var parameterCount = body.Parameters.Count;

            if (parameterCount != 0)
            {
                var methodParameters = new MethodParameterExpression[parameterCount];
                parameters = new IParameter[parameterCount];

                for (var i = 0; i < parameterCount; i++)
                {
                    parameters[i] = methodParameters[i] =
                        new MethodParameterExpression(body.Parameters[i]);
                }

                Parameters = new ReadOnlyCollection<MethodParameterExpression>(methodParameters);
            }
            else
            {
                parameters = Enumerable<IParameter>.EmptyArray;
                Parameters = Enumerable<MethodParameterExpression>.EmptyReadOnlyCollection;
            }

            Body = body.Body;
            Method = new MethodExpressionMethod(this, body, parameters, settings);
        }

        #region Factory Method

        internal static MethodExpression For(Expression expression, TranslationSettings settings)
            => For(null, expression, settings);

        internal static MethodExpression For(
            ClassExpression parent,
            Expression expression,
            TranslationSettings settings)
        {
            if (expression.NodeType == ExpressionType.Lambda)
            {
                return new MethodExpression(parent, (LambdaExpression)expression, settings);
            }

            var lambdaType = expression.HasReturnType()
                ? GetFuncType(expression.Type)
                : GetActionType();

            var lambdaExpression = Lambda(lambdaType, expression);

            return new MethodExpression(parent, lambdaExpression, settings);
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1002) indicating the type of this
        /// <see cref="MethodExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.Method;

        /// <summary>
        /// Gets the type of this <see cref="MethodExpression"/>.
        /// </summary>
        public override Type Type => Method.ReturnType;

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
        /// Gets the <see cref="MethodParameterExpression"/>s describing the parameters of this
        /// <see cref="MethodExpression"/>.
        /// </summary>
        public ReadOnlyCollection<MethodParameterExpression> Parameters { get; }

        /// <summary>
        /// Gets the Expression describing the body of this <see cref="MethodExpression"/>.
        /// </summary>
        public Expression Body { get; }

        internal IMethod Method { get; }

        private int Index => _parent?.Methods.IndexOf(this) ?? 0;

        private class MethodExpressionMethod : IMethod, IMethodNamingContext
        {
            private readonly MethodExpression _method;
            private readonly IParameter[] _parameters;
            private readonly TranslationSettings _settings;
            private string _name;

            public MethodExpressionMethod(
                MethodExpression method,
                LambdaExpression body,
                IParameter[] parameters,
                TranslationSettings settings)
            {
                Body = body;
                _method = method;
                _parameters = parameters;
                _settings = settings;
            }

            public Type DeclaringType => null;

            public bool IsPublic => true;

            public bool IsProtectedInternal => false;

            public bool IsInternal => false;

            public bool IsProtected => false;

            public bool IsPrivate => false;

            public bool IsAbstract => false;

            public bool IsStatic => false;

            public bool IsVirtual => false;

            public string Name
                => _name ??= _settings.MethodNameFactory.Invoke(this);

            public bool IsGenericMethod => false;

            public bool IsExtensionMethod => false;

            public LambdaExpression Body { get; }

            public Type ReturnType => Body.ReturnType;

            public IMethod GetGenericMethodDefinition() => null;

            public Type[] GetGenericArguments()
                => Enumerable<Type>.EmptyArray;

            public IParameter[] GetParameters() => _parameters;

            #region IMethodNamingContext Members

            int IMethodNamingContext.Index => _method.Index;

            #endregion
        }
    }
}