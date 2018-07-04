namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingTryCatchFinallys : TestClassBase
    {
        [Fact]
        public void ShouldTranslateATryWithATopLevelGlobalCatch()
        {
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var exception = Expression.Variable(typeof(Exception), "ex");
            var globalCatch = Expression.Catch(exception, Expression.Empty());
            var tryCatch = Expression.TryCatch(writeHello.Body, globalCatch);

            var translated = ToReadableString(tryCatch);

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch
{
}";
            EXPECTED.TrimStart().ShouldBe(translated);
        }

        [Fact]
        public void ShouldTranslateATryCatch()
        {
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var exception = Expression.Variable(typeof(TimeoutException), "timeoutEx");
            var timeoutCatch = Expression.Catch(exception, Expression.Empty());
            var tryCatch = Expression.TryCatch(writeHello.Body, timeoutCatch);

            var translated = ToReadableString(tryCatch);

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch (TimeoutException)
{
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryWithAFilteredCatch()
        {
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var filter = CreateLambda((TimeoutException timeoutEx) => timeoutEx.Data != null);
            var exception = filter.Parameters.First();
            var timeoutCatch = Expression.Catch(exception, Expression.Empty(), filter.Body);
            var tryCatch = Expression.TryCatch(writeHello.Body, timeoutCatch);

            var translated = ToReadableString(tryCatch);

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch (TimeoutException timeoutEx) when (timeoutEx.Data != null)
{
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryWithATopLevelCatchWithAnExplicitExceptionRethrow()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var rethrow = Expression.Throw(exception);
            var globalCatchAndRethrow = Expression.Catch(exception, rethrow);
            var tryCatch = Expression.TryCatch(writeHello.Body, globalCatchAndRethrow);

            var translated = ToReadableString(tryCatch);

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch
{
    throw;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/1
        [Fact]
        public void ShouldTranslateATryWithATopLevelCatchWithAnImplicitExceptionRethrow()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            var writeHello = CreateLambda(() => Console.WriteLine("Goodbye"));
            var rethrow = Expression.Rethrow();
            var globalCatchAndRethrow = Expression.Catch(exception, rethrow);
            var tryCatch = Expression.TryCatch(writeHello.Body, globalCatchAndRethrow);

            var translated = ToReadableString(tryCatch);

            const string EXPECTED = @"
try
{
    Console.WriteLine(""Goodbye"");
}
catch
{
    throw;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryWithATopLevelCatchWithExceptionUseAndRethrow()
        {
            var writeException = CreateLambda((Exception ex) => Console.Write(ex));
            var exception = writeException.Parameters.First();
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var rethrow = Expression.Throw(exception);
            var writeExceptionAndRethrow = Expression.Block(writeException.Body, rethrow);
            var globalCatch = Expression.Catch(exception, writeExceptionAndRethrow);
            var tryCatch = Expression.TryCatch(writeHello.Body, globalCatch);

            var translated = ToReadableString(tryCatch);

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch (Exception ex)
{
    Console.Write(ex);
    throw;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryWithATopLevelCatchWithAWrappedExceptionThrow()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            var writeBoom = CreateLambda(() => Console.Write("BOOM?"));

            var wrappedException = Expression.New(
                // ReSharper disable once AssignNullToNotNullAttribute
                typeof(InvalidOperationException).GetConstructor(new[] { typeof(string), typeof(Exception) }),
                Expression.Constant("Wrapped!"),
                exception);

            var throwWrapped = Expression.Throw(wrappedException);
            var globalCatch = Expression.Catch(exception, throwWrapped);
            var tryCatch = Expression.TryCatch(writeBoom.Body, globalCatch);

            var translated = ToReadableString(tryCatch);

            const string EXPECTED = @"
try
{
    Console.Write(""BOOM?"");
}
catch (Exception ex)
{
    throw new InvalidOperationException(""Wrapped!"", ex);
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryFinally()
        {
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var writeGoodbye = CreateLambda(() => Console.Write("Goodbye"));
            var tryFinally = Expression.TryCatchFinally(writeHello.Body, writeGoodbye.Body);

            var translated = ToReadableString(tryFinally);

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
finally
{
    Console.Write(""Goodbye"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryFault()
        {
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var writeBoom = CreateLambda(() => Console.Write("Boom"));
            var tryFault = Expression.TryFault(writeHello.Body, writeBoom.Body);

            var translated = ToReadableString(tryFault);

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
fault
{
    Console.Write(""Boom"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryCatchFinally()
        {
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var writeNotSupported = CreateLambda((NotSupportedException ex) => Console.Write("NotSupported!"));
            var notSupportedCatchBlock = Expression.Catch(writeNotSupported.Parameters.First(), writeNotSupported.Body);
            var writeException = CreateLambda((Exception ex) => Console.Write(ex));
            var topLevelCatchBlock = Expression.Catch(writeException.Parameters.First(), writeException.Body);

            var writeFinished = CreateLambda(() => Console.WriteLine("Finished!"));
            var writeGoodbye = CreateLambda(() => Console.Write("Goodbye"));
            var finallyBlock = Expression.Block(writeFinished.Body, writeGoodbye.Body);

            var tryCatchFinally = Expression.TryCatchFinally(writeHello.Body, finallyBlock, notSupportedCatchBlock, topLevelCatchBlock);

            var translated = ToReadableString(tryCatchFinally);

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch (NotSupportedException)
{
    Console.Write(""NotSupported!"");
}
catch (Exception ex)
{
    Console.Write(ex);
}
finally
{
    Console.WriteLine(""Finished!"");
    Console.Write(""Goodbye"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
