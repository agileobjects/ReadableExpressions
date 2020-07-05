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
        private string _name;
        private Type _type;

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
            : this(parent, summaryLines, settings)
        {
            _body = body;

            var method = MethodExpression.For(this, body, settings);
            Methods = method.ToReadOnlyCollection();
            MethodsByReturnType = GetMethodsByReturnType(method);
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            BlockExpression body,
            TranslationSettings settings)
            : this(parent, Enumerable<string>.EmptyArray, settings)
        {
            _body = body;

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

            Methods = methods.ToReadOnlyCollection();
            MethodsByReturnType = GetMethodsByReturnType(methodsByReturnType);
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            string name,
            IList<string> summaryLines,
            IList<MethodExpressionBuilder> methodBuilders,
            TranslationSettings settings)
            : this(parent, summaryLines, settings)
        {
            _name = name;
            
            var methodCount = methodBuilders.Count;

            if (methodCount == 1)
            {
                var method = methodBuilders[0].Build(this, settings);
                _body = method.Definition;
                Methods = method.ToReadOnlyCollection();
                MethodsByReturnType = GetMethodsByReturnType(method);
                return;
            }

            var methods = new MethodExpression[methodCount];
            var methodsByReturnType = new Dictionary<Type, List<MethodExpression>>();

            for (var i = 0; i < methodCount; ++i)
            {
                methods[i] = methodBuilders[i].Build(this, settings);
            }

            _body = Block(methods.ProjectToArray(m => (Expression)m));
            Methods = methods.ToReadOnlyCollection();
            MethodsByReturnType = GetMethodsByReturnType(methodsByReturnType);
        }

        private ClassExpression(
            SourceCodeExpression parent,
            IList<string> summaryLines,
            TranslationSettings settings)
        {
            Parent = parent;
            SummaryLines = summaryLines;
            _settings = settings;
        }

        #region Setup

#if FEATURE_READONLYDICTIONARY
        private ReadOnlyDictionary<Type, ReadOnlyCollection<MethodExpression>> GetMethodsByReturnType(
#else
        private IDictionary<Type, ReadOnlyCollection<MethodExpression>> GetMethodsByReturnType(
#endif
            MethodExpression method)
        {
            return new Dictionary<Type, ReadOnlyCollection<MethodExpression>>(1)
            {
                [method.ReturnType] = Methods
            }
#if FEATURE_READONLYDICTIONARY
            .ToReadOnlyDictionary()
#endif
            ;
        }

#if FEATURE_READONLYDICTIONARY
        private static ReadOnlyDictionary<Type, ReadOnlyCollection<MethodExpression>> GetMethodsByReturnType(
#else
        private static IDictionary<Type, ReadOnlyCollection<MethodExpression>> GetMethodsByReturnType(
#endif
            Dictionary<Type, List<MethodExpression>> methodsByReturnType)
        {
            var readonlyMethodsByReturnType =
                new Dictionary<Type, ReadOnlyCollection<MethodExpression>>(methodsByReturnType.Count);

            foreach (var methodAndReturnType in methodsByReturnType)
            {
                readonlyMethodsByReturnType.Add(
                    methodAndReturnType.Key,
                    methodAndReturnType.Value.ToReadOnlyCollection());
            }

            return readonlyMethodsByReturnType
#if FEATURE_READONLYDICTIONARY
                .ToReadOnlyDictionary()
#endif
            ;
        }

        #endregion

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

        /// <summary>
        /// Gets this <see cref="ClassExpression"/>'s parent <see cref="SourceCodeExpression"/>.
        /// </summary>
        public SourceCodeExpression Parent { get; }

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