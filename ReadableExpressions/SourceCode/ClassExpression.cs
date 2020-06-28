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

    /// <summary>
    /// Represents a class in a piece of source code.
    /// </summary>
    public class ClassExpression : Expression, IClassNamingContext
    {
        private readonly Expression _body;
        private readonly TranslationSettings _settings;
        private Type _type;
        private string _name;

        internal ClassExpression(Expression body, TranslationSettings settings)
            : this(null, body, settings)
        {
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            Expression body,
            TranslationSettings settings)
        {
            Parent = parent;
            _body = body;
            _settings = settings;
            Methods = MethodExpression.For(this, body, settings).ToReadOnlyCollection();
        }

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1001) indicating the type of this
        /// <see cref="ClassExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.Class;

        /// <summary>
        /// Gets the type of this <see cref="ClassExpression"/>, which is the return type of the
        /// Expression from which the main method of the class was created.
        /// </summary>
        public override Type Type
            => _type ??= (_body as LambdaExpression)?.ReturnType ?? _body.Type;

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

        internal SourceCodeExpression Parent { get; }

        /// <summary>
        /// Gets the name of this <see cref="ClassExpression"/>.
        /// </summary>
        public string Name
            => _name ??= _settings.ClassNameFactory.Invoke(Parent, this);

        /// <summary>
        /// Gets the <see cref="MethodExpression"/>s which make up this <see cref="ClassExpression"/>'s
        /// methods.
        /// </summary>
        public ReadOnlyCollection<MethodExpression> Methods { get; }

        /// <summary>
        /// Gets the index of this <see cref="ClassExpression"/> in the set of generated classes.
        /// </summary>
        public int Index => Parent?.Classes.IndexOf(this) ?? 0;

        #region IClassNamingContext Members

        ExpressionType IClassNamingContext.NodeType => _body.NodeType;

        string IClassNamingContext.TypeName
            => Type.GetVariableNameInPascalCase(_settings);

        Expression IClassNamingContext.Body => _body;

        #endregion
    }
}