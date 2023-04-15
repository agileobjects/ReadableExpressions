namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

using System.Linq;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;
#if NET35
using static Microsoft.Scripting.Ast.ExpressionType;
#else
using static System.Linq.Expressions.ExpressionType;
#endif

internal class ArrayInitialisationTranslation :
    InitialisationTranslationBase<Expression>
{
    private ArrayInitialisationTranslation(
        NewArrayExpression arrayInit,
        ITranslationContext context) :
        base(
            NewArrayInit,
            GetNewArrayTranslation(arrayInit, context),
            new ArrayInitializerSetTranslation(arrayInit, context))
    {
    }

    private static INodeTranslation GetNewArrayTranslation(
        NewArrayExpression arrayInit,
        ITranslationContext context)
    {
        bool useImplicitlyTypedArray;

        if (context.Settings.HideImplicitlyTypedArrayTypes)
        {
            var expressionTypes = arrayInit
                .Expressions
                .Project(exp => exp.Type)
                .Distinct()
                .ToList();

            useImplicitlyTypedArray = expressionTypes.Count == 1;
        }
        else
        {
            useImplicitlyTypedArray = false;
        }

        if (useImplicitlyTypedArray)
        {
            return new NewImplicitlyTypedArrayTranslation();
        }

        return new NewBoundedArrayTranslation(arrayInit, context);
    }

    public static INodeTranslation For(
        NewArrayExpression arrayInit,
        ITranslationContext context)
    {
        if (arrayInit.Expressions.None())
        {
            return new NewEmptyBoundedArrayTranslation(arrayInit, context);
        }

        return new ArrayInitialisationTranslation(arrayInit, context);
    }

    private class NewEmptyBoundedArrayTranslation : INodeTranslation
    {
        private readonly INodeTranslation _elementTypeNameTranslation;

        public NewEmptyBoundedArrayTranslation(
            Expression arrayInit,
            ITranslationContext context)
        {
            _elementTypeNameTranslation = context.GetTranslationFor(arrayInit.Type.GetElementType());
        }

        public ExpressionType NodeType => NewArrayInit;

        public int TranslationLength
            => "new ".Length + _elementTypeNameTranslation.TranslationLength + "[0]".Length;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteNewToTranslation();
            _elementTypeNameTranslation.WriteTo(writer);
            writer.WriteToTranslation('[');
            writer.WriteToTranslation(0);
            writer.WriteToTranslation(']');
        }
    }

    private class NewImplicitlyTypedArrayTranslation : INodeTranslation
    {
        public ExpressionType NodeType => NewArrayInit;

        public int TranslationLength => "new[]".Length;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation("new");
            writer.WriteToTranslation("[]");
        }
    }

    private class NewBoundedArrayTranslation : INodeTranslation
    {
        private readonly INodeTranslation _elementTypeNameTranslation;

        public NewBoundedArrayTranslation(
            Expression arrayInit,
            ITranslationContext context)
        {
            _elementTypeNameTranslation = context.GetTranslationFor(arrayInit.Type.GetElementType());
        }

        public ExpressionType NodeType => NewArrayInit;

        public int TranslationLength
            => "new ".Length + _elementTypeNameTranslation.TranslationLength + "[]".Length;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteNewToTranslation();
            _elementTypeNameTranslation.WriteTo(writer);
            writer.WriteToTranslation("[]");
        }
    }
}