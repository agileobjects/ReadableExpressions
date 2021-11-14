namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Extensions;
    using Initialisations;

    internal class AnonymousTypeNewingTranslation : NewingTranslationBase, ITranslation
    {
        private readonly string _typeName;
        private readonly IInitializerSetTranslation _initializers;

        public AnonymousTypeNewingTranslation(NewExpression newing, ITranslationContext context)
            : base(newing, context)
        {
            Type = newing.Type;
            _typeName = context.Settings.AnonymousTypeNameFactory?.Invoke(Type) ?? string.Empty;

            var ctorParameters = newing.Constructor.GetParameters();
            var ctorParameterCount = ctorParameters.Length;

            var initializers = new ITranslation[ctorParameterCount];
            var translationSize = _typeName.Length + "new ".Length;

            for (var i = 0; ;)
            {
                initializers[i] = new AnonymousTypeInitializerTranslation(
                    ctorParameters[i],
                    Parameters[i]);

                ++i;

                if (i == ctorParameterCount)
                {
                    break;
                }
            }

            _initializers = new AnonymousTypeInitializerTranslationSet(initializers, context);

            TranslationSize = translationSize + _initializers.TranslationSize;

            FormattingSize =
                context.GetKeywordFormattingSize() + // <- For 'new'
                _initializers.FormattingSize;
        }

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize()
        {
            _initializers.IsLongTranslation = TranslationSize > 40;

            return _initializers.GetIndentSize();
        }

        public int GetLineCount()
        {
            _initializers.IsLongTranslation = TranslationSize > 40;

            var initializersLineCount = _initializers.GetLineCount();

            return initializersLineCount == 1 ? 1 : initializersLineCount + 1;
        }

        public void WriteTo(TranslationWriter writer)
        {
            _initializers.IsLongTranslation = TranslationSize > 40;

            writer.WriteNewToTranslation();

            if (_typeName.Length != 0)
            {
                writer.WriteTypeNameToTranslation(_typeName);
                writer.WriteSpaceToTranslation();
            }

            _initializers.WriteTo(writer);
        }

        private class AnonymousTypeInitializerTranslation : ITranslation
        {
            private readonly string _memberName;
            private readonly ITranslation _value;

            public AnonymousTypeInitializerTranslation(
                ParameterInfo member,
                ITranslation value)
            {
                _value = value;

                if (value is IParameterTranslation parameter &&
                    parameter.Name == member.Name)
                {
                    _memberName = string.Empty;
                    TranslationSize = value.TranslationSize;
                    return;
                }

                _memberName = member.Name;
                TranslationSize = _memberName.Length + 3 + value.TranslationSize;
            }

            public ExpressionType NodeType => _value.NodeType;

            public Type Type => _value.Type;

            public int TranslationSize { get; }

            public int FormattingSize => _value.FormattingSize;

            public int GetIndentSize() => _value.GetIndentSize();

            public int GetLineCount() => _value.GetLineCount();

            public void WriteTo(TranslationWriter writer)
            {
                if (_memberName != string.Empty)
                {
                    writer.WriteToTranslation(_memberName);
                    writer.WriteToTranslation(" = ");
                }

                _value.WriteTo(writer);
            }
        }

        private class AnonymousTypeInitializerTranslationSet : InitializerSetTranslationBase<ITranslation>
        {
            public AnonymousTypeInitializerTranslationSet(
                IList<ITranslation> initializers,
                ITranslationContext context)
                : base(initializers, context)
            {
            }

            protected override ITranslatable GetTranslation(
                ITranslation initializer,
                ITranslationContext context)
            {
                return initializer;
            }

            public override bool ForceWriteToMultipleLines => false;
        }
    }
}