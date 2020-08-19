namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq;
    using Common;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingTryCatchFinallys : TestClassBase
    {
        [Fact]
        public void ShouldTranslateATryWithATopLevelGlobalCatch()
        {
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var exception = Variable(typeof(Exception), "ex");
            var globalCatch = Catch(exception, Empty());
            var tryCatch = TryCatch(writeHello.Body, globalCatch);

            var translated = tryCatch.ToReadableString();

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch
{
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryCatch()
        {
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var exception = Variable(typeof(TimeoutException), "timeoutEx");
            var timeoutCatch = Catch(exception, Empty());
            var tryCatch = TryCatch(writeHello.Body, timeoutCatch);

            var translated = tryCatch.ToReadableString();

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
            var timeoutCatch = Catch(exception, Empty(), filter.Body);
            var tryCatch = TryCatch(writeHello.Body, timeoutCatch);

            var translated = tryCatch.ToReadableString();

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch (TimeoutException timeoutEx) when timeoutEx.Data != null
{
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATryWithATopLevelCatchWithAnExplicitExceptionRethrow()
        {
            var exception = Variable(typeof(Exception), "ex");
            var writeHello = CreateLambda(() => Console.Write("Hello"));
            var rethrow = Throw(exception);
            var globalCatchAndRethrow = Catch(exception, rethrow);
            var tryCatch = TryCatch(writeHello.Body, globalCatchAndRethrow);

            var translated = tryCatch.ToReadableString();

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
            var exception = Variable(typeof(Exception), "ex");
            var writeHello = CreateLambda(() => Console.WriteLine("Goodbye"));
            var rethrow = Rethrow();
            var globalCatchAndRethrow = Catch(exception, rethrow);
            var tryCatch = TryCatch(writeHello.Body, globalCatchAndRethrow);

            var translated = tryCatch.ToReadableString();

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
            var rethrow = Throw(exception);
            var writeExceptionAndRethrow = Block(writeException.Body, rethrow);
            var globalCatch = Catch(exception, writeExceptionAndRethrow);
            var tryCatch = TryCatch(writeHello.Body, globalCatch);

            var translated = tryCatch.ToReadableString();

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
            var exception = Variable(typeof(Exception), "ex");
            var writeBoom = CreateLambda(() => Console.Write("BOOM?"));

            var wrappedException = New(
                // ReSharper disable once AssignNullToNotNullAttribute
                typeof(InvalidOperationException).GetConstructor(new[] { typeof(string), typeof(Exception) }),
                Constant("Wrapped!"),
                exception);

            var throwWrapped = Throw(wrappedException);
            var globalCatch = Catch(exception, throwWrapped);
            var tryCatch = TryCatch(writeBoom.Body, globalCatch);

            var translated = tryCatch.ToReadableString();

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
            var tryFinally = TryCatchFinally(writeHello.Body, writeGoodbye.Body);

            var translated = tryFinally.ToReadableString();

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
            var tryFault = TryFault(writeHello.Body, writeBoom.Body);

            var translated = tryFault.ToReadableString();

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
            var notSupportedCatchBlock = Catch(writeNotSupported.Parameters.First(), writeNotSupported.Body);
            var writeException = CreateLambda((Exception ex) => Console.Write(ex));
            var topLevelCatchBlock = Catch(writeException.Parameters.First(), writeException.Body);

            var writeFinished = CreateLambda(() => Console.WriteLine("Finished!"));
            var writeGoodbye = CreateLambda(() => Console.Write("Goodbye"));
            var finallyBlock = Block(writeFinished.Body, writeGoodbye.Body);

            var tryCatchFinally = TryCatchFinally(writeHello.Body, finallyBlock, notSupportedCatchBlock, topLevelCatchBlock);

            var translated = tryCatchFinally.ToReadableString();

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
