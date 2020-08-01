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
    using Api;
    using Extensions;
    using ReadableExpressions.Translations.Reflection;

    /// <summary>
    /// Represents a method in a class in a piece of source code.
    /// </summary>
    public class MethodExpression : Expression, IMethodNamingContext
    {
        private readonly TranslationSettings _settings;
        private readonly MethodExpressionMethod _method;
        private List<MethodParameterExpression> _parameters;
        private ReadOnlyCollection<MethodParameterExpression> _readOnlyParameters;

        private MethodExpression(
            ClassExpression parent,
            string name,
            IList<string> summaryLines,
            LambdaExpression definition,
            bool isPublic,
            TranslationSettings settings)
        {
            Parent = parent;
            SummaryLines = summaryLines;
            Definition = definition;
            _settings = settings;

            List<IParameter> parameters;

            var parameterCount = definition.Parameters.Count;

            if (parameterCount != 0)
            {
                _parameters = new List<MethodParameterExpression>(parameterCount);
                parameters = new List<IParameter>(parameterCount);

                for (var i = 0; i < parameterCount; ++i)
                {
                    var parameter = new MethodParameterExpression(definition.Parameters[i]);
                    parameters.Add(parameter);
                    _parameters.Add(parameter); ;
                }
            }
            else
            {
                _parameters = Enumerable<MethodParameterExpression>.EmptyList;
                parameters = Enumerable<IParameter>.EmptyList;
            }

            _method = new MethodExpressionMethod(
                this,
                name,
                parameters,
                isPublic,
                settings);
        }

        #region Factory Methods

        internal static MethodExpression For(
            ClassExpression parent,
            Expression expression,
            TranslationSettings settings,
            bool isPublic = true)
        {
            return For(
                parent,
                name: null,
                summaryLines: Enumerable<string>.EmptyArray,
                expression,
                settings,
                isPublic);
        }

        internal static MethodExpression For(
            ClassExpression parent,
            string name,
            IList<string> summaryLines,
            Expression expression,
            TranslationSettings settings,
            bool isPublic = true)
        {
            var definition = expression.ToLambdaExpression();

            return new MethodExpression(
                parent,
                name,
                summaryLines,
                definition,
                isPublic,
                settings);
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1002) indicating the type of this
        /// <see cref="MethodExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.Method;

        /// <summary>
        /// Gets the type of this <see cref="MethodExpression"/>, which is the return type of the
        /// LambdaExpression from which the method was created.
        /// </summary>
        public override Type Type => ReturnType;

        /// <summary>
        /// Visits this <see cref="MethodExpression"/>'s Body.
        /// </summary>
        /// <param name="visitor">The visitor with which to visit this <see cref="MethodExpression"/>.</param>
        /// <returns>This <see cref="MethodExpression"/>.</returns>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            foreach (var parameter in Parameters)
            {
                visitor.Visit(parameter);
            }

            visitor.Visit(Body);
            return this;
        }

        /// <summary>
        /// Gets this <see cref="MethodExpression"/>'s parent <see cref="ClassExpression"/>.
        /// </summary>
        public ClassExpression Parent { get; }

        /// <summary>
        /// Gets the summary text describing this <see cref="MethodExpression"/>, if set.
        /// </summary>
        public IList<string> SummaryLines { get; }

        /// <summary>
        /// Gets the name of this <see cref="MethodExpression"/>.
        /// </summary>
        public string Name => Method.Name;

        /// <summary>
        /// Gets the return type of this <see cref="MethodExpression"/>, which is the return type
        /// of the LambdaExpression from which the method was created.
        /// </summary>
        public Type ReturnType => Definition.ReturnType;

        /// <summary>
        /// Gets the <see cref="MethodParameterExpression"/>s describing the parameters of this
        /// <see cref="MethodExpression"/>.
        /// </summary>
        public ReadOnlyCollection<MethodParameterExpression> Parameters
            => _readOnlyParameters ??= _parameters.ToReadOnlyCollection();

        /// <summary>
        /// Gets the LambdaExpression describing the parameters and body of this
        /// <see cref="MethodExpression"/>.
        /// </summary>
        public LambdaExpression Definition { get; }

        /// <summary>
        /// Gets the Expression describing the body of this <see cref="MethodExpression"/>.
        /// </summary>
        public Expression Body => Definition.Body;

        internal void AddParameters(IList<ParameterExpression> parameters)
        {
            var parameterCount = parameters.Count;
            var methodParameters = new MethodParameterExpression[parameterCount];
            var iParameters = new IParameter[parameterCount];

            for (var i = 0; i < parameterCount; ++i)
            {
                iParameters[i] = methodParameters[i] =
                    new MethodParameterExpression(parameters[i]);
            }

            if (_parameters.Count == 0)
            {
                _parameters = new List<MethodParameterExpression>();
            }

            _parameters.AddRange(methodParameters);
            _readOnlyParameters = null;

            _method.AddParameters(iParameters);
        }

        internal IMethod Method => _method;

        #region IMethodNamingContext Members

        Type IMethodNamingContext.ReturnType => Type;

        string IMethodNamingContext.ReturnTypeName
            => Type.GetVariableNameInPascalCase(_settings);

        LambdaExpression IMethodNamingContext.MethodLambda => Definition;

        int IMethodNamingContext.Index => Parent?.Methods.IndexOf(this) ?? 0;

        #endregion

        internal class MethodExpressionMethod : IMethod
        {
            private readonly MethodExpression _method;
            private readonly TranslationSettings _settings;
            private string _name;
            private List<IParameter> _parameters;

            public MethodExpressionMethod(
                MethodExpression method,
                string name,
                List<IParameter> parameters,
                bool isPublic,
                TranslationSettings settings)
            {
                _method = method;
                _name = name;
                _parameters = parameters;
                IsPublic = isPublic;
                _settings = settings;
            }

            private ClassExpression Parent => _method.Parent;

            public Type DeclaringType => null;

            public bool IsPublic { get; }

            public bool IsProtectedInternal => false;

            public bool IsInternal => false;

            public bool IsProtected => false;

            public bool IsPrivate => !IsPublic;

            public bool IsAbstract => false;

            public bool IsStatic => false;

            public bool IsVirtual => false;

            public string Name => _name ??= GetName();

            private string GetName()
            {
                return _settings
                    .MethodNameFactory
                    .Invoke(Parent?.Parent, Parent, _method)
                    .ThrowIfInvalidName<InvalidOperationException>("Method");
            }

            public bool IsGenericMethod => false;

            public bool IsExtensionMethod => false;

            public Type ReturnType => _method.Type;

            public IMethod GetGenericMethodDefinition() => null;

            public Type[] GetGenericArguments()
                => Enumerable<Type>.EmptyArray;

            public IList<IParameter> GetParameters() => _parameters;

            public void AddParameters(IList<IParameter> parameters)
            {
                if (_parameters.Count == 0)
                {
                    _parameters = new List<IParameter>(parameters.Count);
                }

                _parameters.AddRange(parameters);
            }
        }
    }
}