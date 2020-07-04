namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
#if NET35
    using Microsoft.Scripting.Ast;
    using Microsoft.Scripting.Utils;
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
            : this(null, Enumerable<string>.EmptyArray, body, settings)
        {
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            Expression body,
            TranslationSettings settings)
            : this(parent, Enumerable<string>.EmptyArray, body, settings)
        {
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            IList<string> summaryLines,
            Expression body,
            TranslationSettings settings)
        {
            Parent = parent;
            SummaryLines = summaryLines;
            _body = body;
            _settings = settings;

            var method = MethodExpression.For(this, body, settings);
            Methods = method.ToReadOnlyCollection();

            MethodsByReturnType = new Dictionary<Type, ReadOnlyCollection<MethodExpression>>(1)
            {
                [method.ReturnType] = Methods
            }
#if FEATURE_READONLYDICTIONARY
                .ToReadOnlyDictionary()
#endif
                ;
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            BlockExpression body,
            TranslationSettings settings)
        {
            Parent = parent;
            SummaryLines = Enumerable<string>.EmptyArray;
            _body = body;
            _settings = settings;

            var expressions = body.Expressions;
            var elementCount = expressions.Count;
            var methods = new MethodExpression[elementCount];
            var methodsByReturnType = new Dictionary<Type, List<MethodExpression>>();

            for (var i = 0; i < elementCount; ++i)
            {
                var expression = expressions[i];

                var method = MethodExpression.For(this, expression, settings);
                methods[i] = method;

                if (!methodsByReturnType.TryGetValue(method.ReturnType, out var typedMethods))
                {
                    methodsByReturnType.Add(
                        method.ReturnType,
                        typedMethods = new List<MethodExpression>());
                }

                typedMethods.Add(method);
            }

            var readonlyMethodsByReturnType =
                new Dictionary<Type, ReadOnlyCollection<MethodExpression>>(methodsByReturnType.Count);

            foreach (var methodAndReturnType in methodsByReturnType)
            {
                readonlyMethodsByReturnType.Add(
                    methodAndReturnType.Key,
                    methodAndReturnType.Value.ToReadOnlyCollection());
            }

            Methods = methods.ToReadOnlyCollection();

            MethodsByReturnType = readonlyMethodsByReturnType
#if FEATURE_READONLYDICTIONARY
                .ToReadOnlyDictionary()
#endif
                ;
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
        /// Visits each of this <see cref="ClassExpression"/>'s <see cref="Methods"/>.
        /// </summary>
        /// <param name="visitor">
        /// The visitor with which to visit this <see cref="ClassExpression"/>'s
        /// <see cref="Methods"/>.
        /// </param>
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
        /// Gets the summary text describing this <see cref="ClassExpression"/>, if set.
        /// </summary>
        public IList<string> SummaryLines { get; }

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
        /// Gets the <see cref="MethodExpression"/>s which make up this <see cref="ClassExpression"/>'s
        /// methods, kyed by their return type.
        /// </summary>
#if FEATURE_READONLYDICTIONARY
        public ReadOnlyDictionary<Type, ReadOnlyCollection<MethodExpression>> MethodsByReturnType { get; }
#else
        public IDictionary<Type, ReadOnlyCollection<MethodExpression>> MethodsByReturnType { get; }
#endif

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