namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using ReadableExpressions.Translations.Reflection;

    /// <summary>
    /// Represents a method parameter in a class in a piece of source code.
    /// </summary>
    public class MethodParameterExpression : Expression, IParameter
    {
        internal MethodParameterExpression(ParameterExpression parameter)
        {
            ParameterExpression = parameter;
        }

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1003) indicating the type of this
        /// <see cref="MethodParameterExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.MethodParameter;

        /// <summary>
        /// Gets the type of this <see cref="MethodParameterExpression"/>.
        /// </summary>
        public override Type Type => ParameterExpression.Type;

        /// <summary>
        /// Visits this <see cref="MethodParameterExpression"/>.
        /// </summary>
        /// <param name="visitor">
        /// The visitor with which to visit this <see cref="MethodParameterExpression"/>.
        /// </param>
        /// <returns>This <see cref="MethodParameterExpression"/>.</returns>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            visitor.Visit(ParameterExpression);
            return this;
        }

        /// <summary>
        /// Gets the ParameterExpression on which this <see cref="MethodParameterExpression"/> is
        /// based.
        /// </summary>
        public ParameterExpression ParameterExpression { get; }

        /// <summary>
        /// Gets the name of the method parameter described by this
        /// <see cref="MethodParameterExpression"/>.
        /// </summary>
        public string Name => ParameterExpression.Name;

        bool IParameter.IsOut => false;

        bool IParameter.IsParamsArray => false;
    }
}