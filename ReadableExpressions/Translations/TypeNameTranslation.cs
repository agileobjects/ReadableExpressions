namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Extensions;

    internal class TypeNameTranslation : ITranslation
    {
        private readonly Type _type;
        private readonly ITranslationContext _context;

        public TypeNameTranslation(Type type, ITranslationContext context)
        {
            _type = type;
            _context = context;
            EstimatedSize = (int)(_type.Name.Length * 1.1);
        }

        public int EstimatedSize { get; }

        public void WriteToTranslation()
        {
            _context.WriteToTranslation(_type.GetFriendlyName(_context.Settings));
        }
    }
}