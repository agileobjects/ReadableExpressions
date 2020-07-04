namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    /// <summary>
    /// Represents a piece of complete source code.
    /// </summary>
    public class SourceCodeExpression : Expression
    {
        internal SourceCodeExpression(Expression content, TranslationSettings settings)
        {
            Namespace = settings.Namespace;
            Content = content;

            ClassExpression @class;

            switch (content.NodeType)
            {
                case ExpressionType.Lambda:
                    @class = new ClassExpression(this, content, settings);
                    Classes = @class.ToReadOnlyCollection();
                    break;

                case ExpressionType.Block when settings.GenerateSingleClass:
                    @class = new ClassExpression(this, (BlockExpression)content, settings);
                    Classes = @class.ToReadOnlyCollection();
                    break;

                case ExpressionType.Block:
                    var expressions = ((BlockExpression)content).Expressions;
                    var elementCount = expressions.Count;
                    var classes = new List<ClassExpression>(elementCount);
                    var summaryLines = Enumerable<string>.EmptyArray;

                    for (var i = 0; i < elementCount; ++i)
                    {
                        var expression = expressions[i];

                        if (expression is CommentExpression comment)
                        {
                            summaryLines = comment.TextLines;
                            continue;
                        }

                        @class = new ClassExpression(this, summaryLines, expression, settings);
                        classes.Add(@class);

                        summaryLines = Enumerable<string>.EmptyArray;
                    }

                    Classes = classes.ToReadOnlyCollection();
                    break;

                default:
                    content = content.ToLambdaExpression();
                    goto case ExpressionType.Lambda;
            }
        }

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1000) indicating the type of this
        /// <see cref="SourceCodeExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.SourceCode;

        /// <summary>
        /// Gets the type of this <see cref="SourceCodeExpression"/> - typeof(void).
        /// </summary>
        public override Type Type => typeof(void);

        /// <summary>
        /// Visits each of this <see cref="SourceCodeExpression"/>'s <see cref="Classes"/>.
        /// </summary>
        /// <param name="visitor">
        /// The visitor with which to visit this <see cref="SourceCodeExpression"/>'s
        /// <see cref="Classes"/>.
        /// </param>
        /// <returns>This <see cref="SourceCodeExpression"/>.</returns>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            foreach (var @class in Classes)
            {
                visitor.Visit(@class);
            }

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
        /// Gets the <see cref="ClassExpression"/>s which describe the classes of this
        /// <see cref="SourceCodeExpression"/>.
        /// </summary>
        public ReadOnlyCollection<ClassExpression> Classes { get; }
    }
}
