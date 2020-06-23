namespace AgileObjects.ReadableExpressions.Translations.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
    using ReadableExpressions.SourceCode;
    using Reflection;

    internal class MethodTranslation : ITranslation
    {
        private readonly MethodExpression _method;
        private readonly ITranslation _bodyTranslation;
        private readonly ITranslatable _definitionTranslation;

        public MethodTranslation(
            MethodExpression method,
            ITranslationContext context)
        {
            _method = method;

            var unscopedVariables = context.GetUnscopedVariablesFor(method);

            var methodObj = unscopedVariables.Any()
                ? new AugmentedMethod(method.Method, unscopedVariables)
                : method.Method;

            _definitionTranslation = new MethodDefinitionTranslation(methodObj, context.Settings);

            var bodyCodeBlock = context
                .GetCodeBlockTranslationFor(method.Body)
                .WithBraces()
                .WithReturnKeyword()
                .WithTermination();

            _bodyTranslation = bodyCodeBlock;

            TranslationSize =
                _definitionTranslation.TranslationSize +
                _bodyTranslation.TranslationSize;

            FormattingSize =
                _definitionTranslation.FormattingSize +
                _bodyTranslation.FormattingSize;
        }

        public ExpressionType NodeType => _method.NodeType;

        public Type Type => _method.Type;

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize()
        {
            return
                _definitionTranslation.GetIndentSize() +
                _bodyTranslation.GetIndentSize();
        }

        public int GetLineCount()
        {
            return
                _definitionTranslation.GetLineCount() +
                _bodyTranslation.GetLineCount();
        }

        public void WriteTo(TranslationWriter writer)
        {
            _definitionTranslation.WriteTo(writer);
            _bodyTranslation.WriteTo(writer);
        }

        #region Helper Members

        private class AugmentedMethod : IMethod
        {
            private readonly IMethod _wrappedMethod;
            private readonly IParameter[] _parameters;

            public AugmentedMethod(
                IMethod wrappedMethod,
                IEnumerable<ParameterExpression> unscopedVariables)
            {
                _wrappedMethod = wrappedMethod;

                _parameters = wrappedMethod
                    .GetParameters()
                    .Concat(unscopedVariables.Select(p => (IParameter)new MethodParameterExpression(p)))
                    .ToArray();
            }

            public Type DeclaringType => _wrappedMethod.DeclaringType;

            public bool IsPublic => _wrappedMethod.IsPublic;

            public bool IsProtectedInternal => _wrappedMethod.IsProtectedInternal;

            public bool IsInternal => _wrappedMethod.IsInternal;

            public bool IsProtected => _wrappedMethod.IsProtected;

            public bool IsPrivate => _wrappedMethod.IsPrivate;

            public bool IsAbstract => _wrappedMethod.IsAbstract;

            public bool IsStatic => _wrappedMethod.IsStatic;

            public bool IsVirtual => _wrappedMethod.IsVirtual;

            public string Name => _wrappedMethod.Name;

            public bool IsGenericMethod => _wrappedMethod.IsGenericMethod;

            public bool IsExtensionMethod => _wrappedMethod.IsExtensionMethod;

            public Type ReturnType => _wrappedMethod.ReturnType;

            public IMethod GetGenericMethodDefinition()
                => _wrappedMethod.GetGenericMethodDefinition();

            public Type[] GetGenericArguments()
                => _wrappedMethod.GetGenericArguments();

            public IParameter[] GetParameters() => _parameters;
        }

        #endregion
    }
}
