namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class ArrayInitialisationTranslation : InitialisationTranslationBase<Expression>
    {
        private ArrayInitialisationTranslation(NewArrayExpression arrayInit, ITranslationContext context)
            : base(
                NewArrayInit,
                GetNewArrayTranslation(arrayInit, context),
                new ArrayInitializerSetTranslation(arrayInit, context))
        {
        }

        private static ITranslation GetNewArrayTranslation(NewArrayExpression arrayInit, ITranslationContext context)
        {
            bool useImplicitlyTypedArray;

            if (context.Settings.HideImplicitlyTypedArrayTypes)
            {
                var expressionTypes = arrayInit
                    .Expressions
                    .Project(exp => exp.Type)
                    .Distinct()
                    .ToArray();

                useImplicitlyTypedArray = expressionTypes.Length == 1;
            }
            else
            {
                useImplicitlyTypedArray = false;
            }

            if (useImplicitlyTypedArray)
            {
                return new NewImplicitlyTypedArrayTranslation(arrayInit, context);
            }

            return new NewBoundedArrayTranslation(arrayInit, context);
        }

        public static ITranslation For(NewArrayExpression arrayInit, ITranslationContext context)
        {
            if (arrayInit.Expressions.Count == 0)
            {
                return new NewEmptyBoundedArrayTranslation(arrayInit, context);
            }

            return new ArrayInitialisationTranslation(arrayInit, context);
        }

        private abstract class NewArrayTranslationBase
        {
            protected NewArrayTranslationBase(Expression arrayInit)
            {
                Type = arrayInit.Type;
            }

            public ExpressionType NodeType => NewArrayInit;

            public Type Type { get; }
        }

        private class NewEmptyBoundedArrayTranslation : NewArrayTranslationBase, ITranslation
        {
            private readonly ITranslation _emptyArrayNewing;

            public NewEmptyBoundedArrayTranslation(Expression arrayInit, ITranslationContext context)
                : base(arrayInit)
            {
                _emptyArrayNewing = context.GetTranslationFor(arrayInit.Type.GetElementType());
                TranslationSize = "new ".Length + _emptyArrayNewing.TranslationSize + "[0]".Length;
                FormattingSize = context.GetKeywordFormattingSize() + _emptyArrayNewing.FormattingSize;
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteNewToTranslation();
                _emptyArrayNewing.WriteTo(buffer);
                buffer.WriteToTranslation('[');
                buffer.WriteToTranslation(0);
                buffer.WriteToTranslation(']');
            }
        }

        private class NewImplicitlyTypedArrayTranslation : NewArrayTranslationBase, ITranslation
        {
            public NewImplicitlyTypedArrayTranslation(Expression arrayInit, ITranslationContext context)
                : base(arrayInit)
            {
                TranslationSize = "new[]".Length;
                FormattingSize = context.GetKeywordFormattingSize();
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteKeywordToTranslation("new");
                buffer.WriteToTranslation("[]");
            }
        }

        private class NewBoundedArrayTranslation : NewArrayTranslationBase, ITranslation
        {
            private readonly ITranslation _emptyArrayNewing;

            public NewBoundedArrayTranslation(Expression arrayInit, ITranslationContext context)
                : base(arrayInit)
            {
                _emptyArrayNewing = context.GetTranslationFor(arrayInit.Type.GetElementType());
                TranslationSize = "new ".Length + _emptyArrayNewing.TranslationSize + "[]".Length;
                FormattingSize = context.GetKeywordFormattingSize() + _emptyArrayNewing.FormattingSize;
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteNewToTranslation();
                _emptyArrayNewing.WriteTo(buffer);
                buffer.WriteToTranslation("[]");
            }
        }
    }
}