namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using NetStandardPolyfills;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingObjectCreations : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAParameterlessNewExpression()
        {
            var createObject = CreateLambda(() => new object());

            var translated = ToReadableString(createObject.Body);

            translated.ShouldBe("new Object()");
        }

        [Fact]
        public void ShouldTranslateAParameterlessNoNamespaceaNewExpression()
        {
            var createObject = CreateLambda(() => new NoNamespace());

            var translated = ToReadableString(createObject.Body);

            translated.ShouldBe("new NoNamespace()");
        }

        [Fact]
        public void ShouldTranslateAFullyQualifiedParameterlessNoNamespaceaNewExpression()
        {
            var createObject = CreateLambda(() => new NoNamespace());

            var translated = ToReadableString(createObject.Body, s => s.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new NoNamespace()");
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithParameters()
        {
            var createToday = CreateLambda(() => new DateTime(2014, 08, 23));

            var translated = ToReadableString(createToday.Body);

            translated.ShouldBe("new DateTime(2014, 8, 23)");
        }

        [Fact]
        public void ShouldTranslateAFullyQualifiedNewExpressionWithParameters()
        {
            var createToday = CreateLambda(() => new DateTime(2018, 11, 17));

            var translated = ToReadableString(createToday.Body, s => s.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new System.DateTime(2018, 11, 17)");
        }

        [Fact]
        public void ShouldTranslateAnEmptyObjectInitialisation()
        {
            var newMemoryStream = New(typeof(MemoryStream));
            var emptyInit = MemberInit(newMemoryStream, new List<MemberBinding>(0));

            var translated = ToReadableString(emptyInit);

            translated.ShouldBe("new MemoryStream()");
        }

        [Fact]
        public void ShouldTranslateANewFullQualifiedNestedGenericTypeExpression()
        {
            var createArray = CreateLambda(() => new NestedType<int>.NestedValue<DateTime>());

            var translated = ToReadableString(createArray.Body, s => s.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new AgileObjects.ReadableExpressions.UnitTests.NestedType<int>.NestedValue<System.DateTime>()");
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithASingleInitialisation()
        {
            var createMemoryStream = CreateLambda(() => new MemoryStream { Position = 0 });

            var translated = ToReadableString(createMemoryStream.Body);

            translated.ShouldBe("new MemoryStream { Position = 0L }");
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithAMultiLineInitialisationValue()
        {
            var writeWat = CreateLambda(() => Console.WriteLine("Wat"));
            var read = CreateLambda<long>(() => Console.Read());

            var newMemoryStream = New(typeof(MemoryStream));
            var positionProperty = newMemoryStream.Type.GetPublicInstanceProperty(nameof(MemoryStream.Position));
            var valueBlock = Block(writeWat.Body, writeWat.Body, read.Body);
            var positionInit = Bind(positionProperty, valueBlock);
            var memoryStreamInit = MemberInit(newMemoryStream, positionInit);

            var translated = ToReadableString(memoryStreamInit);

            const string EXPECTED = @"
new MemoryStream
{
    Position = 
    {
        Console.WriteLine(""Wat"");
        Console.WriteLine(""Wat"");

        return (long)Console.Read();
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithMultipleInitialisations()
        {
            var createMemoryStream = CreateLambda(()
                 => new MemoryStream { Capacity = 10000, Position = 100 });

            var translated = ToReadableString(createMemoryStream.Body);

            const string EXPECTED = @"
new MemoryStream
{
    Capacity = 10000,
    Position = 100L
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithNestedInitialisations()
        {
            var createContactDetails = CreateLambda(() =>
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
                });

            var translated = ToReadableString(createContactDetails.Body);

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
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateANewListExpressionWithAdditions()
        {
            var createList = CreateLambda(() => new List<decimal> { 1m, 2.005m, 3m });

            var translated = ToReadableString(createList.Body);

            translated.ShouldBe("new List<decimal> { 1m, 2.005m, 3m }");
        }

        [Fact]
        public void ShouldTranslateANewDictionaryExpressionWithAdditions()
        {
            var createList = CreateLambda(() => new Dictionary<int, decimal> { { 1, 1.0m }, { 2, 2.0m } });

            var translated = ToReadableString(createList.Body);

            const string EXPECTED = @"
new Dictionary<int, decimal>
{
    { 1, 1m },
    { 2, 2m }
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateANewArrayExpression()
        {
            var createArray = CreateLambda(() => new int[5]);

            var translated = ToReadableString(createArray.Body);

            translated.ShouldBe("new int[5]");
        }

        [Fact]
        public void ShouldTranslateANewGenericTypeArrayExpression()
        {
            var createArray = CreateLambda(() => new List<decimal>[5]);

            var translated = ToReadableString(createArray.Body);

            translated.ShouldBe("new List<decimal>[5]");
        }

        [Fact]
        public void ShouldTranslateANewFullyQualifiedGenericTypeArrayExpression()
        {
            var createArray = CreateLambda(() => new List<decimal>[5]);

            var translated = ToReadableString(createArray.Body, s => s.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new System.Collections.Generic.List<decimal>[5]");
        }

        [Fact]
        public void ShouldTranslateAnImplicitTypeNewArrayExpressionWithAdditions()
        {
            var createArray = CreateLambda(() => new[] { 1.00f, 2.3f, 3.00f });

            var translated = ToReadableString(createArray.Body);

            translated.ShouldBe("new[] { 1f, 2.3f, 3f }");
        }

        [Fact]
        public void ShouldTranslateAConfiguredExplicitTypeNewArrayExpressionWithAdditions()
        {
            var createArray = CreateLambda(() => new[] { 1L, 2L });

            var translated = ToReadableString(createArray.Body, s => s.ShowImplicitArrayTypes);

            translated.ShouldBe("new long[] { 1L, 2L }");
        }

        [Fact]
        public void ShouldTranslateAnExplicitTypeNewArrayExpressionWithAdditions()
        {
            var createDisposables = CreateLambda(() => new IDisposable[]
            {
                new StreamReader(string.Empty),
                new MemoryStream()
            });

            var translated = ToReadableString(createDisposables.Body);

            const string EXPECTED = @"
new IDisposable[]
{
    new StreamReader(string.Empty),
    new MemoryStream()
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnEmptyNewArrayExpression()
        {
            var newArray = NewArrayInit(typeof(int), Enumerable.Empty<Expression>());

            var translated = ToReadableString(newArray);

            translated.ShouldBe("new int[0]");
        }

        [Fact]
        public void ShouldTranslateAStringConstantConstructorParameter()
        {
            var createStringBuilder = CreateLambda(() => new StringBuilder("Hello!"));

            var translated = ToReadableString(createStringBuilder.Body);

            translated.ShouldBe("new StringBuilder(\"Hello!\")");
        }

        [Fact]
        public void ShouldTranslateACharacterConstantConstructorParameter()
        {
            var createStringBuilder = CreateLambda(() => new StringBuilder('f'));

            var translated = ToReadableString(createStringBuilder.Body);

            // Constant character expressions have .Type Int32, so they 
            // can't be differentiated from int constants :(
            translated.ShouldBe($"new StringBuilder({((int)'f')})");
        }

        [Fact]
        public void ShouldTranslateConstantConstructorParameters()
        {
            var createStringBuilder = CreateLambda(() => new StringBuilder(1000, 10000));

            var translated = ToReadableString(createStringBuilder.Body);

            translated.ShouldBe("new StringBuilder(1000, 10000)");
        }

        [Fact]
        public void ShouldTranslateAParameterConstructorParameter()
        {
            var createStringBuilder = CreateLambda((string str) => new StringBuilder(str));

            var translated = ToReadableString(createStringBuilder.Body);

            translated.ShouldBe("new StringBuilder(str)");
        }

        [Fact]
        public void ShouldTranslateMultilineConstructorParameters()
        {
            var consoleRead = CreateLambda(() => Console.Read());

            var catchAll = Catch(typeof(Exception), Default(typeof(int)));
            var tryReadInt = TryCatch(consoleRead.Body, catchAll);

            var createStringBuilder = New(
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

            var translated = ToReadableString(createStringBuilder);

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnAnonymousTypeCreation()
        {
            var anonType = new { ValueString = default(string), ValueInt = default(int) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(string), typeof(int));

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = New(constructor, Constant("How much?!"), Constant(100));

            var translated = ToReadableString(creation);

            translated.ShouldBe("new { ValueString = \"How much?!\", ValueInt = 100 }");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/33
        [Fact]
        public void ShouldTranslateAnAnonymousTypeCreationWithACustomFactory()
        {
            var anonType = new { ValueString = default(string), ValueInt = default(int) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(string), typeof(int));

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = New(constructor, Constant("How much?"), Constant(10));

            var translated = ToReadableString(creation, cfg => cfg.NameAnonymousTypesUsing(t => "MyMagicObject"));

            translated.ShouldBe("new MyMagicObject { ValueString = \"How much?\", ValueInt = 10 }");
        }

        [Fact]
        public void ShouldTranslateAFullyQualfiedAnonymousTypeCreation()
        {
            var anonType = new { ValueString = default(string), TimeSpanValue = default(TimeSpan) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(string), typeof(TimeSpan));

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = New(constructor, Constant("How much?!"), Default(typeof(TimeSpan)));

            var translated = ToReadableString(creation, s => s.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new { ValueString = \"How much?!\", TimeSpanValue = default(System.TimeSpan) }");
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

    internal class NestedType<T>
    {
        internal class NestedValue<TValue>
        {
            public T Type { get; set; }

            public TValue Value { get; set; }
        }
    }

    #endregion
}

public class NoNamespace { }