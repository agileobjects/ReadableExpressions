namespace AgileObjects.ReadableExpressions.Translations;

using System;

internal interface INullKeywordTranslation : INodeTranslation
{
    Type NullType { get; }
}