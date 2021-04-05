﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface ITranslation : ITranslatable
    {
        ExpressionType NodeType { get; }

        Type Type { get; }
    }
}