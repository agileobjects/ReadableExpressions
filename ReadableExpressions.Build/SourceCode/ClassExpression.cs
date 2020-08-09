namespace AgileObjects.ReadableExpressions.Build.SourceCode
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
        private readonly SourceCodeTranslationSettings _settings;
        private readonly List<MethodExpression> _methods;
        private readonly Dictionary<Type, List<MethodExpression>> _methodsByReturnType;
        private ReadOnlyCollection<MethodExpression> _readOnlyMethods;
#if FEATURE_READONLYDICTIONARY
        private ReadOnlyDictionary<Type, ReadOnlyCollection<MethodExpression>> _readOnlyMethodsByReturnType;
#else
        private IDictionary<Type, ReadOnlyCollection<MethodExpression>> _readOnlyMethodsByReturnType;
#endif
        private string _name;
        private Type _type;

        internal ClassExpression(
            SourceCodeExpression parent,
            Expression body,
            SourceCodeTranslationSettings settings)
            : this(parent, Enumerable<string>.EmptyArray, body, settings)
        {
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            IList<string> summaryLines,
            Expression body,
            SourceCodeTranslationSettings settings)
            : this(parent, summaryLines, settings)
        {
            Interfaces = Enumerable<Type>.EmptyReadOnlyCollection;
            _body = body;

            var method = MethodExpression.For(this, body, settings);
            _methods = new List<MethodExpression> { method };

            _methodsByReturnType = new Dictionary<Type, List<MethodExpression>>
            {
                { method.ReturnType, new List<MethodExpression> { method } }
            };
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            BlockExpression body,
            SourceCodeTranslationSettings settings)
            : this(parent, Enumerable<string>.EmptyArray, settings)
        {
            Interfaces = Enumerable<Type>.EmptyReadOnlyCollection;
            _body = body;
            _methods = new List<MethodExpression>();
            _methodsByReturnType = new Dictionary<Type, List<MethodExpression>>();

            foreach (var expression in body.Expressions)
            {
                var method = MethodExpression.For(this, expression, settings);
                _methods.Add(method);
                AddTypedMethod(method);
            }
        }

        internal ClassExpression(
            SourceCodeExpression parent,
            string name,
            IList<Type> interfaceTypes,
            IList<string> summaryLines,
            IList<MethodExpressionBuilder> methodBuilders,
            SourceCodeTranslationSettings settings)
            : this(parent, summaryLines, settings)
        {
            _name = name;

            Interfaces = interfaceTypes != null
                ? new ReadOnlyCollection<Type>(interfaceTypes)
                : Enumerable<Type>.EmptyReadOnlyCollection;

            var methodCount = methodBuilders.Count;

            if (methodCount == 1)
            {
                var method = methodBuilders[0].Build(this, settings);
                _body = method.Definition;
                _methods = new List<MethodExpression> { method };
                _methodsByReturnType = new Dictionary<Type, List<MethodExpression>>
                {
                    { method.ReturnType, new List<MethodExpression> { method } }
                };
                return;
            }

            _methods = new List<MethodExpression>();
            _methodsByReturnType = new Dictionary<Type, List<MethodExpression>>();

            foreach (var methodBuilder in methodBuilders)
            {
                var method = methodBuilder.Build(this, settings);
                _methods.Add(method);
                AddTypedMethod(method);
            }

            _body = Block(_methods.ProjectToArray(m => (Expression)m));
        }

        private ClassExpression(
            SourceCodeExpression parent,
            IList<string> summaryLines,
            SourceCodeTranslationSettings settings)
        {
            Parent = parent;
            SummaryLines = summaryLines;
            _settings = settings;
        }

        #region Setup

        private void AddTypedMethod(MethodExpression method)
        {
            if (!_methodsByReturnType.TryGetValue(method.ReturnType, out var typedMethods))
            {
                _methodsByReturnType.Add(
                    method.ReturnType,
                    typedMethods = new List<MethodExpression>());
            }

            typedMethods.Add(method);
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
        public string Name => _name ??= GetName();

        private string GetName()
        {
            return _settings
                .ClassNameFactory
                .Invoke(Parent, this)
                .ThrowIfInvalidName<InvalidOperationException>("Class");
        }

        /// <summary>
        /// Gets the interface types implemented by this <see cref="ClassExpression"/>.
        /// </summary>
        public ReadOnlyCollection<Type> Interfaces { get; }

        internal void AddMethod(MethodExpression method)
        {
            _methods.Add(method);
            _readOnlyMethods = null;

            AddTypedMethod(method);
            _readOnlyMethodsByReturnType = null;
        }

        /// <summary>
        /// Gets the <see cref="MethodExpression"/>s which make up this <see cref="ClassExpression"/>'s
        /// methods.
        /// </summary>
        public ReadOnlyCollection<MethodExpression> Methods
            => _readOnlyMethods ??= _methods.ToReadOnlyCollection();

        /// <summary>
        /// Gets the <see cref="MethodExpression"/>s which make up this <see cref="ClassExpression"/>'s
        /// methods, kyed by their return type.
        /// </summary>
#if FEATURE_READONLYDICTIONARY
        public ReadOnlyDictionary<Type, ReadOnlyCollection<MethodExpression>> MethodsByReturnType
#else
        public IDictionary<Type, ReadOnlyCollection<MethodExpression>> MethodsByReturnType
#endif
            => _readOnlyMethodsByReturnType ??= GetMethodsByReturnType();

#if FEATURE_READONLYDICTIONARY
        private ReadOnlyDictionary<Type, ReadOnlyCollection<MethodExpression>> GetMethodsByReturnType()
#else
        private IDictionary<Type, ReadOnlyCollection<MethodExpression>> GetMethodsByReturnType()
#endif
        {
            var readonlyMethodsByReturnType =
                new Dictionary<Type, ReadOnlyCollection<MethodExpression>>(_methodsByReturnType.Count);

            foreach (var methodAndReturnType in _methodsByReturnType)
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