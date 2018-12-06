namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal class TypeNameTranslation : ITranslation
    {
        private const string _object = "object";
        private readonly TranslationSettings _translationSettings;
        private readonly bool _isObject;
        private bool _writeObjectTypeName;

        public TypeNameTranslation(Type type, ITranslationContext context)
        {
            Type = type;
            _translationSettings = context.Settings;
            _isObject = type == typeof(object);

            if (_isObject)
            {
                EstimatedSize = _object.Length;
                return;
            }

            if (type.FullName == null)
            {
                return;
            }

            if (_translationSettings.FullyQualifyTypeNames && (type.Namespace != null))
            {
                EstimatedSize = type.Namespace.Length;
            }

            EstimatedSize += type.GetSubstitutionOrNull()?.Length ?? type.Name.Length;

            while (type.IsNested)
            {
                type = type.DeclaringType;
                EstimatedSize += type.Name.Length;
            }
        }

        public ExpressionType NodeType => ExpressionType.Constant;

        public Type Type { get; }

        public int EstimatedSize { get; }

        public TypeNameTranslation WithObjectTypeName()
        {
            if (_isObject)
            {
                _writeObjectTypeName = true;
            }

            return this;
        }

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_isObject)
            {
                buffer.WriteToTranslation(_writeObjectTypeName ? "Object" : _object);
                return;
            }

            buffer.WriteFriendlyName(Type, _translationSettings);
        }
    }
}