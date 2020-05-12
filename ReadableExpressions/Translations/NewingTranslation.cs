namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using NetStandardPolyfills;
    using Interfaces;

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
            private readonly int _ctorParameterCount;

            public AnonymousTypeNewingTranslation(NewExpression newing, ITranslationContext context)
                : base(newing, context)
            {
                Type = newing.Type;
                _typeName = context.Settings.AnonymousTypeNameFactory?.Invoke(Type) ?? string.Empty;
                _ctorParameters = newing.Constructor.GetParameters();
                _ctorParameterCount = _ctorParameters.Length;

                TranslationSize =
                    _typeName.Length +
                    _ctorParameters.Sum(p => p.Name.Length + 3 /* <- for ' = ' */) +
                     Parameters.TranslationSize +
                    "new {  }".Length;

                FormattingSize =
                    context.GetKeywordFormattingSize() +
                    context.GetVariableFormattingSize() * _ctorParameterCount;
            }

            public Type Type { get; }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetLineCount()
            {
                switch (_ctorParameterCount)
                {
                    case 0:
                        return 1;

                    case 1:
                        return Parameters[0].GetLineCount();
                }

                var lineCount = 1;

                for (var i = 0; i < _ctorParameterCount; ++i)
                {
                    var parameterLineCount = Parameters[i].GetLineCount();

                    if (parameterLineCount > 1)
                    {
                        lineCount += parameterLineCount - 1;
                    }
                }

                return lineCount;
            }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteNewToTranslation();

                if (_typeName.Length != 0)
                {
                    buffer.WriteTypeNameToTranslation(_typeName);
                    buffer.WriteSpaceToTranslation();
                }

                buffer.WriteToTranslation("{ ");

                if (_ctorParameterCount != 0)
                {
                    for (var i = 0; ;)
                    {
                        buffer.WriteToTranslation(_ctorParameters[i].Name);
                        buffer.WriteToTranslation(" = ");
                        Parameters[i].WriteTo(buffer);

                        ++i;

                        if (i == _ctorParameterCount)
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

                TranslationSize =
                    "new ()".Length +
                    _typeNameTranslation.TranslationSize +
                     Parameters.TranslationSize;

                FormattingSize =
                     context.GetKeywordFormattingSize() +
                    _typeNameTranslation.FormattingSize +
                     Parameters.FormattingSize;
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetLineCount()
            {
                return Parameters.None
                    ? _typeNameTranslation.GetLineCount()
                    : Parameters.GetLineCount();
            }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteNewToTranslation();
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