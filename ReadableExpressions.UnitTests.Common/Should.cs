using System;

namespace AgileObjects.ReadableExpressions.UnitTests.Common
{
    public static class Should
    {
        public static TException Throw<TException>(Action test)
            where TException : Exception
        {
            return Throw<TException>(() =>
            {
                test.Invoke();

                return new object();
            });
        }

        public static TException Throw<TException>(Func<object> testFunc)
            where TException : Exception
        {
            try
            {
                testFunc.Invoke();
            }
            catch (TException ex)
            {
                return ex;
            }

            throw new Exception("Expected exception of type " + typeof(TException).Name);
        }
    }
}
