namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
    using Net35;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif

    public abstract class TestClassBase
    {
#if NET35
        protected static LambdaExpression CreateLambda(LinqExp.Expression<Action> linqLambda)
            => (LambdaExpression)LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg>(LinqExp.Expression<Action<TArg>> linqLambda)
            => (LambdaExpression)LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg1, TArg2>(LinqExp.Expression<Action<TArg1, TArg2>> linqLambda)
            => (LambdaExpression)LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        internal static LambdaExpression CreateLambda<TReturn>(LinqExp.Expression<Func<TReturn>> linqLambda)
            => (LambdaExpression)LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg, TReturn>(LinqExp.Expression<Func<TArg, TReturn>> linqLambda)
            => (LambdaExpression)LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg1, TArg2, TReturn>(LinqExp.Expression<Func<TArg1, TArg2, TReturn>> linqLambda)
            => (LambdaExpression)LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg1, TArg2, TArg3, TReturn>(LinqExp.Expression<Func<TArg1, TArg2, TArg3, TReturn>> linqLambda)
            => (LambdaExpression)LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        internal static string ToReadableString(Expression expression, Func<TranslationSettings, TranslationSettings> configuration = null)
            => expression.ToReadableString();
#else
        protected static LambdaExpression CreateLambda(Expression<Action> lambda) => lambda;

        protected static LambdaExpression CreateLambda<TArg>(Expression<Action<TArg>> lambda) => lambda;
        
        protected static LambdaExpression CreateLambda<TArg1, TArg2>(Expression<Action<TArg1, TArg2>> lambda) => lambda;

        internal static LambdaExpression CreateLambda<TReturn>(Expression<Func<TReturn>> lambda) => lambda;

        protected static LambdaExpression CreateLambda<TArg, TReturn>(Expression<Func<TArg, TReturn>> lambda) => lambda;

        protected static LambdaExpression CreateLambda<TArg1, TArg2, TReturn>(Expression<Func<TArg1, TArg2, TReturn>> lambda) => lambda;

        protected static LambdaExpression CreateLambda<TArg1, TArg2, TArg3, TReturn>(Expression<Func<TArg1, TArg2, TArg3, TReturn>> lambda) => lambda;

        internal static string ToReadableString(Expression expression, Func<TranslationSettings, TranslationSettings> configuration = null) 
            => expression.ToReadableString(configuration);
#endif
    }
}