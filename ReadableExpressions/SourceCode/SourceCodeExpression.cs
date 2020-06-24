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
    /// Represents a piece of complete source code.
    /// </summary>
    public class SourceCodeExpression : Expression
    {
        internal SourceCodeExpression(Expression content, TranslationSettings settings)
        {
            Namespace = "GeneratedExpressionCode";
            Content = content;

            switch (content.NodeType)
            {
                case ExpressionType.Lambda:
                    Elements = new ReadOnlyCollection<Expression>(
                        new Expression[] { new ClassExpression((LambdaExpression)content, settings) });
                    break;
            }
        }

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1000) indicating the type of this
        /// <see cref="SourceCodeExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.SourceCode;

        /// <summary>
        /// Gets the type of this <see cref="SourceCodeExpression"/> - typeof(string).
        /// </summary>
        public override Type Type => typeof(string);

        /// <summary>
        /// Visits each of this <see cref="SourceCodeExpression"/>'s Elements.
        /// </summary>
        /// <param name="visitor">The visitor with which to visit this <see cref="SourceCodeExpression"/>.</param>
        /// <returns>This <see cref="SourceCodeExpression"/>.</returns>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            visitor.Visit(Elements);
            return this;
        }

        /// <summary>
        /// Gets the Expression on which this <see cref="SourceCodeExpression"/> is based.
        /// </summary>
        public Expression Content { get; }

        /// <summary>
        /// Gets the namespace to which the source code represented by this
        /// <see cref="SourceCodeExpression"/> belongs.
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// Gets the Expressions which describe the elements of this <see cref="SourceCodeExpression"/>.
        /// </summary>
        public ReadOnlyCollection<Expression> Elements { get; }
    }
}
