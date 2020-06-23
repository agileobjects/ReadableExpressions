namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
    using Translations;
#else
    using System.Linq.Expressions;
#endif
#if NET35
    using LinqExp = System.Linq.Expressions;
#endif

    public abstract class TestClassBase
    {
#if NET35
        protected static LambdaExpression CreateLambda(LinqExp.Expression<Action> linqLambda)
            => LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg>(LinqExp.Expression<Action<TArg>> linqLambda)
            => LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg1, TArg2>(LinqExp.Expression<Action<TArg1, TArg2>> linqLambda)
            => LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        internal static LambdaExpression CreateLambda<TReturn>(LinqExp.Expression<Func<TReturn>> linqLambda)
            => LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg, TReturn>(LinqExp.Expression<Func<TArg, TReturn>> linqLambda)
            => LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg1, TArg2, TReturn>(LinqExp.Expression<Func<TArg1, TArg2, TReturn>> linqLambda)
            => LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        protected static LambdaExpression CreateLambda<TArg1, TArg2, TArg3, TReturn>(LinqExp.Expression<Func<TArg1, TArg2, TArg3, TReturn>> linqLambda)
            => LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        internal static string ToReadableString(Expression expression, Func<ITranslationSettings, ITranslationSettings> configuration = null)
            => expression.ToReadableString(configuration);
#else
        protected static LambdaExpression CreateLambda(Expression<Action> lambda) => lambda;

        protected static LambdaExpression CreateLambda<TArg>(Expression<Action<TArg>> lambda) => lambda;
        
        protected static LambdaExpression CreateLambda<TArg1, TArg2>(Expression<Action<TArg1, TArg2>> lambda) => lambda;

        internal static LambdaExpression CreateLambda<TReturn>(Expression<Func<TReturn>> lambda) => lambda;

        protected static LambdaExpression CreateLambda<TArg, TReturn>(Expression<Func<TArg, TReturn>> lambda) => lambda;

        protected static LambdaExpression CreateLambda<TArg1, TArg2, TReturn>(Expression<Func<TArg1, TArg2, TReturn>> lambda) => lambda;

        protected static LambdaExpression CreateLambda<TArg1, TArg2, TArg3, TReturn>(Expression<Func<TArg1, TArg2, TArg3, TReturn>> lambda) => lambda;

        internal static string ToReadableString(Expression expression, Func<ITranslationSettings, ITranslationSettings> configuration = null) 
            => expression.ToReadableString(configuration);
#endif
    }
}