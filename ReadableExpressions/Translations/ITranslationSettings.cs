namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Formatting;

    internal interface ITranslationSettings
    {
        ITranslationFormatter Formatter { get; }

        string Indent { get; }

        bool FullyQualifyTypeNames { get; }
        
        Func<Type, string> AnonymousTypeNameFactory { get; }
    }
}