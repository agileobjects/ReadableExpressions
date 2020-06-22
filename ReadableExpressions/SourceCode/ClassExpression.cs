namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.ObjectModel;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Represents a class in a piece of source code.
    /// </summary>
    public class ClassExpression : Expression
    {
        internal ClassExpression(LambdaExpression singleMethodContent)
        {
            Name = "GeneratedExpressionClass";

            Methods = new ReadOnlyCollection<MethodExpression>(
                new[] { new MethodExpression(singleMethodContent) });
        }

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1001) indicating the type of this
        /// <see cref="ClassExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.Class;

        /// <summary>
        /// Gets the type of this <see cref="ClassExpression"/> - typeof(string).
        /// </summary>
        public override Type Type => typeof(string);

        /// <summary>
        /// Visits each of this <see cref="ClassExpression"/>'s Methods.
        /// </summary>
        /// <param name="visitor">The visitor with which to visit this <see cref="ClassExpression"/>.</param>
        /// <returns>This <see cref="ClassExpression"/>.</returns>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            foreach (var method in Methods)
            {
                visitor.Visit(method);
            }

            return this;
        }

        /// <summary>
        /// Gets the name of this <see cref="ClassExpression"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="MethodExpression"/>s which make up this <see cref="ClassExpression"/>'s
        /// methods.
        /// </summary>
        public ReadOnlyCollection<MethodExpression> Methods { get; }
    }
}