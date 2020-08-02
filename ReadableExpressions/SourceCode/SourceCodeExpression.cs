namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Translations;

    /// <summary>
    /// Represents a piece of complete source code.
    /// </summary>
    public class SourceCodeExpression : Expression
    {
        private readonly TranslationSettings _settings;

        internal SourceCodeExpression(Expression content, TranslationSettings settings)
            : this(settings)
        {
            Content = content;

            switch (content.NodeType)
            {
                case ExpressionType.Lambda:
                    Classes = new ClassExpression(this, content, settings).ToReadOnlyCollection();
                    break;

                case ExpressionType.Block:
                    var block = (BlockExpression)content;

                    if (!HasValidExpressions(block, out var isNestedBlocks))
                    {
                        throw InvalidBlockStructure();
                    }

                    IEnumerable<ClassExpression> classes;

                    if (isNestedBlocks)
                    {
                        classes = block
                            .Expressions
                            .SelectMany(exp => EnumerateClasses((BlockExpression)exp));
                    }
                    else
                    {
                        classes = EnumerateClasses(block);
                    }

                    Classes = classes.ToList().ToReadOnlyCollection();
                    break;

                default:
                    content = content.ToLambdaExpression();
                    goto case ExpressionType.Lambda;
            }
        }

        #region Setup

        private static bool HasValidExpressions(BlockExpression block, out bool isNestedBlocks)
        {
            const ExpressionType COMMENT = (ExpressionType)SourceCodeExpressionType.Comment;

            isNestedBlocks = false;
            var isCommentsAndMethods = false;
            var previousExpressionType = default(ExpressionType);

            foreach (var expression in block.Expressions)
            {
                var expressionType = expression.NodeType;

                switch (expressionType)
                {
                    case ExpressionType.Block:
                        if (isCommentsAndMethods ||
                            HasValidExpressions((BlockExpression)expression, out isNestedBlocks))
                        {
                            return false;
                        }

                        isNestedBlocks = true;
                        continue;

                    case COMMENT:
                    case ExpressionType.Lambda:
                        if (isNestedBlocks)
                        {
                            return false;
                        }

                        isCommentsAndMethods = true;

                        if (expressionType == COMMENT &&
                            previousExpressionType != default &&
                            previousExpressionType != ExpressionType.Lambda)
                        {
                            return false;
                        }

                        break;

                    default:
                        return false;
                }

                previousExpressionType = expressionType;
            }

            return true;
        }

        private IEnumerable<ClassExpression> EnumerateClasses(BlockExpression block)
        {
            if (_settings.GenerateSingleClass)
            {
                yield return new ClassExpression(this, block, _settings);
                yield break;
            }

            var expressions = block.Expressions;
            var elementCount = expressions.Count;
            var summaryLines = Enumerable<string>.EmptyArray;

            for (var i = 0; i < elementCount; ++i)
            {
                var expression = expressions[i];

                if (expression.IsComment())
                {
                    summaryLines = ((CommentExpression)expression).TextLines;
                    continue;
                }

                yield return new ClassExpression(this, summaryLines, expression, _settings);
                summaryLines = Enumerable<string>.EmptyArray;
            }
        }

        private static NotSupportedException InvalidBlockStructure()
        {
            return new NotSupportedException(
                "Source code can only be generated from BlockExpressions made up of " +
                "LambdaExpressions with optional Comments, or BlockExpressions made " +
                "up of LambdaExpressions with optional Comments");
        }

        #endregion

        internal SourceCodeExpression(
            IList<ClassExpressionBuilder> classBuilders,
            TranslationSettings settings)
            : this(settings)
        {
            var classCount = classBuilders.Count;

            if (classCount == 1)
            {
                var @class = classBuilders[0].Build(this, settings);
                Content = @class;
                Classes = @class.ToReadOnlyCollection();
                return;
            }

            var classes = new ClassExpression[classCount];

            for (var i = 0; i < classCount; ++i)
            {
                classes[i] = classBuilders[i].Build(this, settings);
            }

            Content = Block(classes.ProjectToArray(cls => (Expression)cls));
            Classes = classes.ToReadOnlyCollection();
        }

        private SourceCodeExpression(TranslationSettings settings)
        {
            _settings = settings;
            Namespace = settings.Namespace;
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

        /// <summary>
        /// Translates this <see cref="SourceCodeExpression"/> to a complete source-code string,
        /// formatted as one or more classes with one or more methods in a namespace.
        /// </summary>
        /// <returns>
        /// The translated <see cref="SourceCodeExpression"/>, formatted as one or more classes with
        /// one or more methods in a namespace.
        /// </returns>
        public string ToSourceCode()
            => new SourceCodeExpressionTranslation(this, _settings).GetTranslation();
    }
}
