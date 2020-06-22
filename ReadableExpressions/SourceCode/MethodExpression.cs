namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Extensions;
    using Translations.Reflection;

    /// <summary>
    /// Represents a method in a class in a piece of source code.
    /// </summary>
    public class MethodExpression : Expression
    {
        internal MethodExpression(LambdaExpression bodyLambda)
        {
            Body = bodyLambda.Body;
            Method = new MethodExpressionMethod(bodyLambda.Body);
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

            public MethodExpressionMethod(Expression body)
            {
                _body = body;
                Name = "Get" + ReturnType.GetFriendlyName().ToPascalCase();
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

            public ParameterInfo[] GetParameters()
                => Enumerable<ParameterInfo>.EmptyArray;
        }
    }
}