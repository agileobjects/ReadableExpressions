using System;

namespace AgileObjects.ReadableExpressions.Translations
{
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
            private readonly ParameterInfo[] _ctorParameters;

            public AnonymousTypeNewingTranslation(NewExpression newing, ITranslationContext context)
                : base(newing, context)
            {
                Type = newing.Type;
                _ctorParameters = newing.Constructor.GetParameters();
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
            {
                return _ctorParameters.Sum(p => p.Name.Length) +
                       (3 * _ctorParameters.Length) + // <- for ' = '
                        Parameters.EstimatedSize +
                       "new {  }".Length;
            }

            public Type Type { get; }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                context.WriteToTranslation("new { ");

                if (_ctorParameters.Length != 0)
                {
                    for (var i = 0; ;)
                    {
                        context.WriteToTranslation(_ctorParameters[i].Name);
                        context.WriteToTranslation(" = ");
                        Parameters[i].WriteTo(context);

                        if (++i == _ctorParameters.Length)
                        {
                            break;
                        }

                        context.WriteToTranslation(", ");
                    }
                }

                context.WriteToTranslation(" }");
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

            public void WriteTo(ITranslationContext context)
            {
                context.WriteToTranslation("new ");
                _typeNameTranslation.WriteTo(context);

                if (_omitParenthesesIfParameterless && Parameters.None)
                {
                    return;
                }

                Parameters.WithParentheses().WriteTo(context);
            }

            public Type Type => _typeNameTranslation.Type;
        }
    }
}