namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Extensions;

    internal class TypeNameTranslation : ITranslation
    {
        private readonly Type _type;

        public TypeNameTranslation(Type type)
        {
            _type = type;
            EstimatedSize = (int)(_type.Name.Length * 1.1);
        }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation(_type.GetFriendlyName(context.Settings));
        }
    }
}