namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Interfaces;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using NetStandardPolyfills;

    internal static class NewingTranslation
    {
        public static ITranslation For(
            NewExpression newing,
            ITranslationContext context,
            bool omitParenthesesIfParameterless = false)
        {
            if (newing.Type.IsAnonymous())
            {
                return new AnonymousTypeNewingTranslation(newing, context);
            }

            return new StandardNewingTranslation(newing, context, omitParenthesesIfParameterless);
        }

        private abstract class NewingTranslationBase
        {
            protected NewingTranslationBase(NewExpression newing, ITranslationContext context)
            {
                Parameters = new ParameterSetTranslation(newing.Arguments, context);
            }

            public ExpressionType NodeType => ExpressionType.New;

            protected ParameterSetTranslation Parameters { get; }
        }

        private class AnonymousTypeNewingTranslation : NewingTranslationBase, ITranslation
        {
            private readonly string _typeName;
            private readonly ParameterInfo[] _ctorParameters;

            public AnonymousTypeNewingTranslation(NewExpression newing, ITranslationContext context)
                : base(newing, context)
            {
                Type = newing.Type;
                _typeName = context.Settings.AnonymousTypeNameFactory?.Invoke(Type) ?? string.Empty;
                _ctorParameters = newing.Constructor.GetParameters();
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
            {
                return _typeName.Length +
                       _ctorParameters.Sum(p => p.Name.Length) +
                       (3 * _ctorParameters.Length) + // <- for ' = '
                        Parameters.EstimatedSize +
                       "new {  }".Length;
            }

            public Type Type { get; }

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteToTranslation("new ");

                if (_typeName.Length != 0)
                {
                    buffer.WriteToTranslation(_typeName);
                    buffer.WriteSpaceToTranslation();
                }

                buffer.WriteToTranslation("{ ");

                if (_ctorParameters.Length != 0)
                {
                    for (var i = 0; ;)
                    {
                        buffer.WriteToTranslation(_ctorParameters[i].Name);
                        buffer.WriteToTranslation(" = ");
                        Parameters[i].WriteTo(buffer);

                        if (++i == _ctorParameters.Length)
                        {
                            break;
                        }

                        buffer.WriteToTranslation(", ");
                    }
                }

                buffer.WriteToTranslation(" }");
            }
        }

        private class StandardNewingTranslation : NewingTranslationBase, ITranslation
        {
            private readonly ITranslation _typeNameTranslation;
            private readonly bool _omitParenthesesIfParameterless;

            public StandardNewingTranslation(
                NewExpression newing,
                ITranslationContext context,
                bool omitParenthesesIfParameterless)
                : base(newing, context)
            {
                _omitParenthesesIfParameterless = omitParenthesesIfParameterless;
                _typeNameTranslation = context.GetTranslationFor(newing.Type).WithObjectTypeName();
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
            {
                return _typeNameTranslation.EstimatedSize +
                       Parameters.EstimatedSize +
                       "new ()".Length;
            }

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteToTranslation("new", TokenType.Keyword);
                buffer.WriteToTranslation(' ');
                _typeNameTranslation.WriteTo(buffer);

                if (_omitParenthesesIfParameterless && Parameters.None)
                {
                    return;
                }

                Parameters.WithParentheses().WriteTo(buffer);
            }

            public Type Type => _typeNameTranslation.Type;
        }
    }
}