namespace AgileObjects.ReadableExpressions.Translations;

using System.Collections.Generic;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using System.Reflection;
using Extensions;
using Initialisations;

internal class AnonymousTypeNewingTranslation : NewingTranslationBase, INodeTranslation
{
    private readonly string _typeName;
    private readonly IInitializerSetTranslation _initializers;

    public AnonymousTypeNewingTranslation(
        NewExpression newing,
        ITranslationContext context) :
        base(newing, context)
    {
        _typeName = context.Settings.AnonymousTypeNameFactory?.Invoke(newing.Type) ?? string.Empty;

        var ctorParameters = newing.Constructor.GetParameters();
        var ctorParameterCount = ctorParameters.Length;

        var initializers = new ITranslation[ctorParameterCount];

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

        _initializers =
            new AnonymousTypeInitializerTranslationSet(this, initializers, context);
    }

    public int TranslationLength
        => "new ".Length + _typeName.Length + _initializers.TranslationLength;

    public void WriteTo(TranslationWriter writer)
    {
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
        private readonly INodeTranslation _value;

        public AnonymousTypeInitializerTranslation(
            ParameterInfo member,
            INodeTranslation value)
        {
            _value = value;

            if (value is IParameterTranslation parameter &&
                parameter.Name == member.Name)
            {
                _memberName = string.Empty;
                return;
            }

            _memberName = member.Name;
        }

        public int TranslationLength => _memberName == string.Empty
            ? _value.TranslationLength
            : _memberName.Length + " = ".Length + _value.TranslationLength;

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

    private class AnonymousTypeInitializerTranslationSet :
        InitializerSetTranslationBase<ITranslation>
    {
        public AnonymousTypeInitializerTranslationSet(
            ITranslation parent,
            IList<ITranslation> initializers,
            ITranslationContext context) :
            base(initializers, context)
        {
            Parent = parent;
        }

        protected override ITranslation GetTranslationFor(
            ITranslation initializer,
            ITranslationContext context)
        {
            return initializer;
        }
    }
}