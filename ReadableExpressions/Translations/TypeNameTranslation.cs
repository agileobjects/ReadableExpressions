namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Extensions;

    internal class TypeNameTranslation : ITranslation
    {
        private const string _object = "object";

        private readonly Type _type;
        private readonly bool _isObject;

        public TypeNameTranslation(Type type)
        {
            _isObject = type == typeof(object);

            if (_isObject)
            {
                EstimatedSize = _object.Length;
                return;
            }

            _type = type;
            EstimatedSize = (int)(_type.Name.Length * 1.1);
        }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_isObject)
            {
                context.WriteToTranslation(_object);
                return;
            }

            context.WriteToTranslation(_type.GetFriendlyName(context.Settings));
        }
    }
}