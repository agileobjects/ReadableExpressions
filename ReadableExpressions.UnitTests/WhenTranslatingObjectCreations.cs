namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Text;
    using NetStandardPolyfills;
    using Xunit;

    public class WhenTranslatingObjectCreations
    {
        [Fact]
        public void ShouldTranslateAParameterlessNewExpression()
        {
            Expression<Func<object>> createObject = () => new object();

            var translated = createObject.Body.ToReadableString();

            Assert.Equal("new Object()", translated);
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithParameters()
        {
            Expression<Func<DateTime>> createToday = () => new DateTime(2014, 08, 23);

            var translated = createToday.Body.ToReadableString();

            Assert.Equal("new DateTime(2014, 8, 23)", translated);
        }

        [Fact]
        public void ShouldTranslateAnEmptyObjectInitialisation()
        {
            var newMemoryStream = Expression.New(typeof(MemoryStream));
            var emptyInit = Expression.MemberInit(newMemoryStream, new List<MemberBinding>(0));

            var translated = emptyInit.ToReadableString();

            Assert.Equal("new MemoryStream()", translated);
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithASingleInitialisation()
        {
            Expression<Func<MemoryStream>> createMemoryStream = () => new MemoryStream { Position = 0 };

            var translated = createMemoryStream.Body.ToReadableString();

            Assert.Equal("new MemoryStream { Position = 0L }", translated);
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithAMultiLineInitialisationValue()
        {
            Expression<Action> writeWat = () => Console.WriteLine("Wat");
            Expression<Func<long>> read = () => Console.Read();

            var newMemoryStream = Expression.New(typeof(MemoryStream));
            var positionProperty = newMemoryStream.Type.GetProperty("Position");
            var valueBlock = Expression.Block(writeWat.Body, writeWat.Body, read.Body);
            // ReSharper disable once AssignNullToNotNullAttribute
            var positionInit = Expression.Bind(positionProperty, valueBlock);
            var memoryStreamInit = Expression.MemberInit(newMemoryStream, positionInit);

            var translated = memoryStreamInit.ToReadableString();

            const string EXPECTED = @"
new MemoryStream
{
    Position = 
    {
        Console.WriteLine(""Wat"");
        Console.WriteLine(""Wat"");

        return ((long)Console.Read());
    }
}";
            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithMultipleInitialisations()
        {
            Expression<Func<MemoryStream>> createMemoryStream =
                () => new MemoryStream { Capacity = 10000, Position = 100 };

            var translated = createMemoryStream.Body.ToReadableString();

            const string EXPECTED = @"
new MemoryStream
{
    Capacity = 10000,
    Position = 100L
}";
            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithNestedInitialisations()
        {
            Expression<Func<ContactDetails>> createContactDetails = () =>
                new ContactDetails
                {
                    Name = "Kermit",
                    Address =
                    {
                        HouseNumber = 1,
                        Postcode = new Postcode("VX 3 9FX")
                    },
                    PhoneNumbers =
                    {
                        "01234567890",
                        "07896543210"
                    }
                };

            var translated = createContactDetails.Body.ToReadableString();

            const string EXPECTED = @"
new ContactDetails
{
    Name = ""Kermit"",
    Address =
    {
        HouseNumber = 1,
        Postcode = new Postcode(""VX 3 9FX"")
    },
    PhoneNumbers =
    {
        ""01234567890"",
        ""07896543210""
    }
}";
            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateANewListExpressionWithAdditions()
        {
            Expression<Func<List<decimal>>> createList =
                () => new List<decimal> { 1m, 2.005m, 3m };

            var translated = createList.Body.ToReadableString();

            Assert.Equal("new List<decimal> { 1m, 2.005m, 3m }", translated);
        }

        [Fact]
        public void ShouldTranslateANewDictionaryExpressionWithAdditions()
        {
            Expression<Func<Dictionary<int, decimal>>> createList =
                () => new Dictionary<int, decimal> { { 1, 1.0m }, { 2, 2.0m } };

            var translated = createList.Body.ToReadableString();

            const string EXPECTED = @"
new Dictionary<int, decimal>
{
    { 1, 1m },
    { 2, 2m }
}";

            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateANewArrayExpression()
        {
            Expression<Func<int[]>> createArray = () => new int[5];

            var translated = createArray.Body.ToReadableString();

            Assert.Equal("new int[5]", translated);
        }

        [Fact]
        public void ShouldTranslateANewGenericTypeArrayExpression()
        {
            Expression<Func<Tuple<decimal>[]>> createArray = () => new Tuple<decimal>[5];

            var translated = createArray.Body.ToReadableString();

            Assert.Equal("new Tuple<decimal>[5]", translated);
        }

        [Fact]
        public void ShouldTranslateAnImplicitTypeNewArrayExpressionWithAdditions()
        {
            Expression<Func<float[]>> createArray = () => new[] { 1.00f, 2.3f, 3.00f };

            var translated = createArray.Body.ToReadableString();

            Assert.Equal("new[] { 1f, 2.3f, 3f }", translated);
        }

        [Fact]
        public void ShouldTranslateAnExplicitTypeNewArrayExpressionWithAdditions()
        {
            Expression<Func<IDisposable[]>> createDisposables = () => new IDisposable[]
            {
                new StreamReader(string.Empty),
                new MemoryStream()
            };

            var translated = createDisposables.Body.ToReadableString();

            const string EXPECTED = @"
new IDisposable[]
{
    new StreamReader(string.Empty),
    new MemoryStream()
}";

            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateAnEmptyNewArrayExpression()
        {
            var newArray = Expression.NewArrayInit(typeof(int), new List<Expression>(0));

            var translated = newArray.ToReadableString();

            Assert.Equal("new int[0]", translated);
        }

        [Fact]
        public void ShouldTranslateAStringConstantConstructorParameter()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder("Hello!");

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.Equal("new StringBuilder(\"Hello!\")", translated);
        }

        [Fact]
        public void ShouldTranslateACharacterConstantConstructorParameter()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder('f');

            var translated = createStringBuilder.Body.ToReadableString();

            // Constant character expressions have .Type Int32, so they 
            // can't be differentiated from int constants :(
            Assert.Equal($"new StringBuilder({((int)'f')})", translated);
        }

        [Fact]
        public void ShouldTranslateConstantConstructorParameters()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder(1000, 10000);

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.Equal("new StringBuilder(1000, 10000)", translated);
        }

        [Fact]
        public void ShouldTranslateAParameterConstructorParameter()
        {
            Expression<Func<string, StringBuilder>> createStringBuilder = str => new StringBuilder(str);

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.Equal("new StringBuilder(str)", translated);
        }

        [Fact]
        public void ShouldTranslateMultilineConstructorParameters()
        {
            Expression<Func<int>> consoleRead = () => Console.Read();

            var catchAll = Expression.Catch(typeof(Exception), Expression.Default(typeof(int)));
            var tryReadInt = Expression.TryCatch(consoleRead.Body, catchAll);

            var createStringBuilder = Expression.New(
                typeof(StringBuilder).GetPublicInstanceConstructor(typeof(int), typeof(int)),
                tryReadInt,
                tryReadInt);

            const string EXPECTED = @"
new StringBuilder(
    {
        try
        {
            return Console.Read();
        }
        catch
        {
            return default(int);
        }
    },
    {
        try
        {
            return Console.Read();
        }
        catch
        {
            return default(int);
        }
    })";

            var translated = createStringBuilder.ToReadableString();

            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateAnAnonymousTypeCreation()
        {
            var anonType = new { ValueString = default(string), ValueInt = default(int) }.GetType();
            var constructor = anonType.GetConstructor(new[] { typeof(string), typeof(int) });

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = Expression.New(constructor, Expression.Constant("How much?!"), Expression.Constant(100));

            var translated = creation.ToReadableString();

            Assert.Equal("new { ValueString = \"How much?!\", ValueInt = 100 }", translated);
        }
    }

    #region Helper Classes

    internal class Postcode
    {
        public Postcode(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    internal class Address
    {
        public int HouseNumber { get; set; }

        public Postcode Postcode { get; set; }
    }

    internal class ContactDetails
    {
        public string Name { get; set; }

        public Address Address { get; set; }

        public List<string> PhoneNumbers { get; set; }
    }

    #endregion
}
