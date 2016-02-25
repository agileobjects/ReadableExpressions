namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingTryCatchFinallys
    {
        [TestMethod]
        public void ShouldTranslateATryWithATopLevelGlobalCatch()
        {
            Expression<Action> writeHello = () => Console.Write("Hello");
            var exception = Expression.Variable(typeof(Exception), "ex");
            var globalCatch = Expression.Catch(exception, Expression.Empty());
            var tryCatch = Expression.TryCatch(writeHello.Body, globalCatch);

            var translated = tryCatch.ToReadableString();

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch
{
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateATryCatch()
        {
            Expression<Action> writeHello = () => Console.Write("Hello");
            var exception = Expression.Variable(typeof(TimeoutException), "timeoutEx");
            var timeoutCatch = Expression.Catch(exception, Expression.Empty());
            var tryCatch = Expression.TryCatch(writeHello.Body, timeoutCatch);

            var translated = tryCatch.ToReadableString();

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch (TimeoutException)
{
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateATryWithAFilteredCatch()
        {
            Expression<Action> writeHello = () => Console.Write("Hello");
            Expression<Func<TimeoutException, bool>> filter = timeoutEx => timeoutEx.Data != null;
            var exception = filter.Parameters.First();
            var timeoutCatch = Expression.Catch(exception, Expression.Empty(), filter.Body);
            var tryCatch = Expression.TryCatch(writeHello.Body, timeoutCatch);

            var translated = tryCatch.ToReadableString();

            const string EXPECTED = @"
try
{
    Console.Write(""Hello"");
}
catch (TimeoutException timeoutEx) when (timeoutEx.Data != null)
{
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateATryWithATopLevelCatchWithExceptionRethrow()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            Expression<Action> writeHello = () => Console.Write("Hello");
            var rethrow = Expression.Throw(exception);
            var globalCatchAndRethrow = Expression.Catch(exception, rethrow);
            var tryCatch = Expression.TryCatch(writeHello.Body, globalCatchAndRethrow);

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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateATryWithATopLevelCatchWithExceptionUseAndRethrow()
        {
            Expression<Action<Exception>> writeException = ex => Console.Write(ex);
            var exception = writeException.Parameters.First();
            Expression<Action> writeHello = () => Console.Write("Hello");
            var rethrow = Expression.Throw(exception);
            var writeExceptionAndRethrow = Expression.Block(writeException.Body, rethrow);
            var globalCatch = Expression.Catch(exception, writeExceptionAndRethrow);
            var tryCatch = Expression.TryCatch(writeHello.Body, globalCatch);

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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateATryFinally()
        {
            Expression<Action> writeHello = () => Console.Write("Hello");
            Expression<Action> writeGoodbye = () => Console.Write("Goodbye");
            var tryFinally = Expression.TryCatchFinally(writeHello.Body, writeGoodbye.Body);

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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateATryFault()
        {
            Expression<Action> writeHello = () => Console.Write("Hello");
            Expression<Action> writeBoom = () => Console.Write("Boom");
            var tryFault = Expression.TryFault(writeHello.Body, writeBoom.Body);

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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateATryCatchFinally()
        {
            Expression<Action> writeHello = () => Console.Write("Hello");
            Expression<Action<NotSupportedException>> writeNotSupported = ex => Console.Write("NotSupported!");
            var notSupportedCatchBlock = Expression.Catch(writeNotSupported.Parameters.First(), writeNotSupported.Body);
            Expression<Action<Exception>> writeException = ex => Console.Write(ex);
            var topLevelCatchBlock = Expression.Catch(writeException.Parameters.First(), writeException.Body);

            Expression<Action> writeFinished = () => Console.WriteLine("Finished!");
            Expression<Action> writeGoodbye = () => Console.Write("Goodbye");
            var finallyBlock = Expression.Block(writeFinished.Body, writeGoodbye.Body);

            var tryCatchFinally = Expression.TryCatchFinally(writeHello.Body, finallyBlock, notSupportedCatchBlock, topLevelCatchBlock);

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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
