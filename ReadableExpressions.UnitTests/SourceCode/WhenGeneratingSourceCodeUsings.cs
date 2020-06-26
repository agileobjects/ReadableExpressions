namespace AgileObjects.ReadableExpressions.UnitTests.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using NetStandardPolyfills;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenGeneratingSourceCodeUsings : TestClassBase
    {
        [Fact]
        public void ShouldIncludeAUsingFromADefaultExpression()
        {
            var getDefaultDate = Lambda<Func<DateTime>>(Default(typeof(DateTime)));

            var translated = getDefaultDate.ToSourceCode();

            const string EXPECTED = @"
using System;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public DateTime GetDateTime()
        {
            return default(DateTime);
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAUsingFromATypeOfExpression()
        {
            var getDefaultDate = Lambda<Func<Type>>(Constant(typeof(Stream)));

            var translated = getDefaultDate.ToSourceCode();

            const string EXPECTED = @"
using System;
using System.IO;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public Type GetType()
        {
            return typeof(Stream);
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAUsingFromAnObjectNewing()
        {
            var createStringBuilder = Lambda<Func<object>>(New(typeof(StringBuilder)));

            var translated = createStringBuilder.ToSourceCode();

            const string EXPECTED = @"
using System.Text;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public object GetObject()
        {
            return new StringBuilder();
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAUsingFromAGenericTypeArgument()
        {
            var comparerType = typeof(Comparer<StringBuilder>);
            var defaultComparer = Property(null, comparerType, "Default");
            var comparerNotNull = NotEqual(defaultComparer, Default(defaultComparer.Type));
            var comparerCheckLambda = Lambda<Func<bool>>(comparerNotNull);

            var translated = comparerCheckLambda.ToSourceCode();

            const string EXPECTED = @"
using System.Collections.Generic;
using System.Text;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public bool GetBool()
        {
            return Comparer<StringBuilder>.Default != null;
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAUsingFromAGenericMethodArgument()
        {
            var helperVariable = Variable(typeof(TestHelper), "helper");
            var newHelper = New(typeof(TestHelper));
            var populateHelper = Assign(helperVariable, newHelper);

            var method = typeof(TestHelper)
                .GetPublicInstanceMethod("GetTypeName")
                .MakeGenericMethod(typeof(Regex));

            var methodCall = Call(helperVariable, method);
            var lambdaBody = Block(new[] { helperVariable }, populateHelper, methodCall);
            var lambda = Lambda<Func<string>>(lambdaBody);

            var translated = lambda.ToSourceCode();

            const string EXPECTED = @"
using System.Text.RegularExpressions;
using AgileObjects.ReadableExpressions.UnitTests.SourceCode;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public string GetString()
        {
            var helper = new WhenGeneratingSourceCodeUsings.TestHelper();

            return helper.GetTypeName<Regex>();
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotIncludeAUsingFromAnImplicitGenericMethodArgument()
        {
            var helperVariable = Variable(typeof(TestHelper), "helper");
            var newHelper = New(typeof(TestHelper));
            var populateHelper = Assign(helperVariable, newHelper);

            var method = typeof(TestHelper)
                .GetPublicInstanceMethods("GetHashCode")
                .First(m => m.IsGenericMethod)
                .MakeGenericMethod(typeof(Regex));

            var methodCall = Call(helperVariable, method, Property(helperVariable, "Regex"));
            var lambdaBody = Block(new[] { helperVariable }, populateHelper, methodCall);
            var lambda = Lambda<Func<int>>(lambdaBody);

            var translated = lambda.ToSourceCode();

            const string EXPECTED = @"
using AgileObjects.ReadableExpressions.UnitTests.SourceCode;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public int GetInt()
        {
            var helper = new WhenGeneratingSourceCodeUsings.TestHelper();

            return helper.GetHashCode(helper.Regex);
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAUsingFromAStaticMemberAccessExpression()
        {
            var dateTimeNow = Property(null, typeof(DateTime), nameof(DateTime.Now));
            var dateTimeTicks = Property(dateTimeNow, nameof(DateTime.Ticks));
            var getDefaultDate = Lambda<Func<long>>(dateTimeTicks);

            var translated = getDefaultDate.ToSourceCode();

            const string EXPECTED = @"
using System;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public long GetLong()
        {
            return DateTime.Now.Ticks;
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeUsingsFromMethodArgumentTypes()
        {
            var stringBuilderMatchesRegex = CreateLambda(
                (Regex re, StringBuilder sb) => re.IsMatch(sb.ToString()));

            var translated = stringBuilderMatchesRegex.ToSourceCode();

            const string EXPECTED = @"
using System.Text;
using System.Text.RegularExpressions;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public bool GetBool
        (
            Regex re,
            StringBuilder sb
        )
        {
            return re.IsMatch(sb.ToString());
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeUsingsFromMethodGenericArgumentTypes()
        {
            var joinListItems = CreateLambda(
                (Func<IList<string>> listFactory) => string.Join(", ", listFactory.Invoke().ToArray()));

            var translated = joinListItems.ToSourceCode();

            const string EXPECTED = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public string GetString
        (
            Func<IList<string>> listFactory
        )
        {
            return string.Join("", "", listFactory.Invoke().ToArray());
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAUsingFromAnExtensionMethod()
        {
            var joinListItems = CreateLambda(
                (string[] strings) => strings.Select(int.Parse).ToList());

            var translated = joinListItems.ToSourceCode();

            const string EXPECTED = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public List<int> GetInts
        (
            string[] strings
        )
        {
            return strings.Select(int.Parse).ToList();
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotDuplicateUsings()
        {
            var stringBuilderContainsOther = CreateLambda(
                (StringBuilder sb1, StringBuilder sb2) => sb1.ToString().Contains(sb2.ToString()));

            var translated = stringBuilderContainsOther.ToSourceCode();

            const string EXPECTED = @"
using System.Text;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public bool GetBool
        (
            StringBuilder sb1,
            StringBuilder sb2
        )
        {
            return sb1.ToString().Contains(sb2.ToString());
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        #region Helper Members

        public class TestHelper
        {
            public Regex Regex => null;

            public string GetTypeName<T>() => typeof(T).Name;

            public int GetHashCode<T>(T obj) => obj.GetHashCode();
        }

        #endregion
    }
}