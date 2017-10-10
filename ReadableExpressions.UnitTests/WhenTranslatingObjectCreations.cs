namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingObjectCreations
    {
        [TestMethod]
        public void ShouldTranslateAParameterlessNewExpression()
        {
            Expression<Func<object>> createObject = () => new object();

            var translated = createObject.Body.ToReadableString();

            Assert.AreEqual("new Object()", translated);
        }

        [TestMethod]
        public void ShouldTranslateANewExpressionWithParameters()
        {
            Expression<Func<DateTime>> createToday = () => new DateTime(2014, 08, 23);

            var translated = createToday.Body.ToReadableString();

            Assert.AreEqual("new DateTime(2014, 8, 23)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnEmptyObjectInitialisation()
        {
            var newMemoryStream = Expression.New(typeof(MemoryStream));
            var emptyInit = Expression.MemberInit(newMemoryStream, new List<MemberBinding>(0));

            var translated = emptyInit.ToReadableString();

            Assert.AreEqual("new MemoryStream()", translated);
        }

        [TestMethod]
        public void ShouldTranslateANewExpressionWithASingleInitialisation()
        {
            Expression<Func<MemoryStream>> createMemoryStream = () => new MemoryStream { Position = 0 };

            var translated = createMemoryStream.Body.ToReadableString();

            Assert.AreEqual("new MemoryStream { Position = 0L }", translated);
        }

        [TestMethod]
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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateANewListExpressionWithAdditions()
        {
            Expression<Func<List<decimal>>> createList =
                () => new List<decimal> { 1m, 2.005m, 3m };

            var translated = createList.Body.ToReadableString();

            Assert.AreEqual("new List<decimal> { 1m, 2.005m, 3m }", translated);
        }

        [TestMethod]
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

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateANewArrayExpression()
        {
            Expression<Func<int[]>> createArray = () => new int[5];

            var translated = createArray.Body.ToReadableString();

            Assert.AreEqual("new int[5]", translated);
        }

        [TestMethod]
        public void ShouldTranslateANewGenericTypeArrayExpression()
        {
            Expression<Func<Tuple<decimal>[]>> createArray = () => new Tuple<decimal>[5];

            var translated = createArray.Body.ToReadableString();

            Assert.AreEqual("new Tuple<decimal>[5]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnImplicitTypeNewArrayExpressionWithAdditions()
        {
            Expression<Func<float[]>> createArray = () => new[] { 1.00f, 2.3f, 3.00f };

            var translated = createArray.Body.ToReadableString();

            Assert.AreEqual("new[] { 1f, 2.3f, 3f }", translated);
        }

        [TestMethod]
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

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAnEmptyNewArrayExpression()
        {
            var newArray = Expression.NewArrayInit(typeof(int), new List<Expression>(0));

            var translated = newArray.ToReadableString();

            Assert.AreEqual("new int[0]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAStringConstantConstructorParameter()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder("Hello!");

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.AreEqual("new StringBuilder(\"Hello!\")", translated);
        }

        [TestMethod]
        public void ShouldTranslateACharacterConstantConstructorParameter()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder('f');

            var translated = createStringBuilder.Body.ToReadableString();

            // Constant character expressions have .Type Int32, so they 
            // can't be differentiated from int constants :(
            Assert.AreEqual($"new StringBuilder({((int)'f')})", translated);
        }

        [TestMethod]
        public void ShouldTranslateConstantConstructorParameters()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder(1000, 10000);

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.AreEqual("new StringBuilder(1000, 10000)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAParameterConstructorParameter()
        {
            Expression<Func<string, StringBuilder>> createStringBuilder = str => new StringBuilder(str);

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.AreEqual("new StringBuilder(str)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnAnonymousTypeCreation()
        {
            var anonType = new { ValueString = default(string), ValueInt = default(int) }.GetType();
            var constructor = anonType.GetConstructor(new[] { typeof(string), typeof(int) });

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = Expression.New(constructor, Expression.Constant("How much?!"), Expression.Constant(100));

            var translated = creation.ToReadableString();

            Assert.AreEqual("new { ValueString = \"How much?!\", ValueInt = 100 }", translated);
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
