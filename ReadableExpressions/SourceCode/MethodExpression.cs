namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
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
        private MethodExpression(LambdaExpression bodyLambda)
        {
            Body = bodyLambda.Body;
            Method = new MethodExpressionMethod(bodyLambda);
        }

        internal static MethodExpression For(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Lambda)
            {
                return new MethodExpression((LambdaExpression)expression);
            }

            var lambdaType = expression.HasReturnType()
                ? GetFuncType(expression.Type)
                : GetActionType();

            var lambdaExpression = Lambda(lambdaType, expression);

            return new MethodExpression(lambdaExpression);
        }

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1002) indicating the type of this
        /// <see cref="MethodExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.Method;

        /// <summary>
        /// Gets the type of this <see cref="MethodExpression"/>.
        /// </summary>
        public override Type Type => Body.Type;

        /// <summary>
        /// Visits this <see cref="MethodExpression"/>'s Body.
        /// </summary>
        /// <param name="visitor">The visitor with which to visit this <see cref="MethodExpression"/>.</param>
        /// <returns>This <see cref="MethodExpression"/>.</returns>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            visitor.Visit(Body);
            return this;
        }

        /// <summary>
        /// Gets the Expression describing the body of this <see cref="MethodExpression"/>.
        /// </summary>
        public Expression Body { get; }

        internal IMethod Method { get; }

        private class MethodExpressionMethod : IMethod
        {
            private readonly Expression _body;
            private readonly IParameter[] _parameters;

            public MethodExpressionMethod(LambdaExpression definition)
            {
                _body = definition.Body;

                Name = _body.HasReturnType()
                    ? "Get" + ReturnType.GetFriendlyName().ToPascalCase()
                    : "DoAction";

                _parameters = definition
                    .Parameters
                    .ProjectToArray(p => (IParameter)new MethodExpressionParameter(p));
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

            public string Name { get; }

            public bool IsGenericMethod => false;

            public bool IsExtensionMethod => false;

            public Type ReturnType => _body.Type;

            public IMethod GetGenericMethodDefinition() => null;

            public Type[] GetGenericArguments()
                => Enumerable<Type>.EmptyArray;

            public IParameter[] GetParameters() => _parameters;

            private class MethodExpressionParameter : IParameter
            {
                private readonly ParameterExpression _parameter;

                public MethodExpressionParameter(ParameterExpression parameter)
                {
                    _parameter = parameter;
                }

                public Type Type => _parameter.Type;

                public string Name => _parameter.Name;

                public bool IsOut => false;

                public bool IsParamsArray => false;
            }
        }
    }
}