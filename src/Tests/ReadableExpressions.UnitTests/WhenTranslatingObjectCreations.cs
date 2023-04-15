﻿namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Common;
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

            var translated = createObject.Body.ToReadableString();

            translated.ShouldBe("new Object()");
        }

        [Fact]
        public void ShouldTranslateAParameterlessNoNamespaceaNewExpression()
        {
            var createObject = CreateLambda(() => new NoNamespace());

            var translated = createObject.Body.ToReadableString();

            translated.ShouldBe("new NoNamespace()");
        }

        [Fact]
        public void ShouldTranslateAFullyQualifiedParameterlessNoNamespaceaNewExpression()
        {
            var createObject = CreateLambda(() => new NoNamespace());

            var translated = createObject.Body.ToReadableString(stgs => stgs.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new NoNamespace()");
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithParameters()
        {
            var createToday = CreateLambda(() => new DateTime(2014, 08, 23));

            var translated = createToday.Body.ToReadableString();

            translated.ShouldBe("new DateTime(2014, 8, 23)");
        }

        [Fact]
        public void ShouldTranslateAFullyQualifiedNewExpressionWithParameters()
        {
            var createToday = CreateLambda(() => new DateTime(2018, 11, 17));

            var translated = createToday.Body.ToReadableString(stgs => stgs.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new System.DateTime(2018, 11, 17)");
        }

        [Fact]
        public void ShouldTranslateAnEmptyObjectInitialisation()
        {
            var newMemoryStream = New(typeof(MemoryStream));
            var emptyInit = MemberInit(newMemoryStream, new List<MemberBinding>(0));

            var translated = emptyInit.ToReadableString();

            translated.ShouldBe("new MemoryStream()");
        }

        [Fact]
        public void ShouldTranslateANewFullQualifiedNestedGenericTypeExpression()
        {
            var createArray = CreateLambda(() => new NestedType<int>.NestedValue<DateTime>());

            var translated = createArray.Body.ToReadableString(stgs => stgs.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new AgileObjects.ReadableExpressions.UnitTests.NestedType<int>.NestedValue<System.DateTime>()");
        }

        [Fact]
        public void ShouldTranslateANewExpressionWithASingleInitialisation()
        {
            var createMemoryStream = CreateLambda(() => new MemoryStream { Position = 0 });

            var translated = createMemoryStream.Body.ToReadableString();

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

            var translated = memoryStreamInit.ToReadableString();

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
        public void ShouldTranslateANewAnonymousTypeExpressionWithAMultiLineCtorValue()
        {
            var writeBlah = CreateLambda(() => Console.WriteLine("Blah"));
            var read = CreateLambda<long>(() => Console.Read());

            var anonType = new { LongValue = default(long) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(long));
            var valueBlock = Block(writeBlah.Body, writeBlah.Body, read.Body);
            var newAnonType = New(constructor, valueBlock);

            var translated = newAnonType.ToReadableString();

            const string EXPECTED = @"
new 
{
    LongValue = 
    {
        Console.WriteLine(""Blah"");
        Console.WriteLine(""Blah"");

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

            var translated = createMemoryStream.Body.ToReadableString();

            const string EXPECTED = @"
new MemoryStream
{
    Capacity = 10000,
    Position = 100L
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateANewAonymousTypeExpressionWithMultipleInitialisations()
        {
            var anonType = new { Value1 = default(string), Value2 = default(string) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(string), typeof(string));

            var propertyValue = Constant("This is a long value and it should wrap", typeof(string));

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = New(constructor, propertyValue, propertyValue);

            var translated = creation.ToReadableString();

            const string EXPECTED = @"
new 
{
    Value1 = ""This is a long value and it should wrap"",
    Value2 = ""This is a long value and it should wrap""
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
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateANewListExpressionWithAdditions()
        {
            var createList = CreateLambda(() => new List<decimal> { 1m, 2.005m, 3m });

            var translated = createList.Body.ToReadableString();

            translated.ShouldBe("new List<decimal> { 1m, 2.005m, 3m }");
        }

        [Fact]
        public void ShouldTranslateANewDictionaryExpressionWithAdditions()
        {
            var createList = CreateLambda(() => new Dictionary<int, decimal> { { 1, 1.0m }, { 2, 2.0m } });

            var translated = createList.Body.ToReadableString();

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

            var translated = createArray.Body.ToReadableString();

            translated.ShouldBe("new int[5]");
        }

        [Fact]
        public void ShouldTranslateANewGenericTypeArrayExpression()
        {
            var createArray = CreateLambda(() => new List<decimal>[5]);

            var translated = createArray.Body.ToReadableString();

            translated.ShouldBe("new List<decimal>[5]");
        }

        [Fact]
        public void ShouldTranslateANewFullyQualifiedGenericTypeArrayExpression()
        {
            var createArray = CreateLambda(() => new List<decimal>[5]);

            var translated = createArray.Body.ToReadableString(stgs => stgs.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new System.Collections.Generic.List<decimal>[5]");
        }

        [Fact]
        public void ShouldTranslateAnImplicitTypeNewArrayExpressionWithAdditions()
        {
            var createArray = CreateLambda(() => new[] { 1.00f, 2.3f, 3.00f });

            var translated = createArray.Body.ToReadableString();

            translated.ShouldBe("new[] { 1f, 2.3f, 3f }");
        }

        [Fact]
        public void ShouldTranslateAConfiguredExplicitTypeNewArrayExpressionWithAdditions()
        {
            var createArray = CreateLambda(() => new[] { 1L, 2L });

            var translated = createArray.Body.ToReadableString(stgs => stgs.ShowImplicitArrayTypes);

            translated.ShouldBe("new long[] { 1L, 2L }");
        }

        [Fact]
        public void ShouldTranslateAnExplicitTypeNewArrayExpressionWithAdditions()
        {
            var createDisposables = CreateLambda(() => new IDisposable[]
            {
                new StreamReader(File.OpenRead(@"C:\temp\my-file.txt")),
                new MemoryStream()
            });

            var translated = createDisposables.Body.ToReadableString();

            const string EXPECTED = @"
new IDisposable[]
{
    new StreamReader(File.OpenRead(""C:\\temp\\my-file.txt"")),
    new MemoryStream()
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnEmptyNewArrayExpression()
        {
            var newArray = NewArrayInit(typeof(int), Enumerable.Empty<Expression>());

            var translated = newArray.ToReadableString();

            translated.ShouldBe("new int[0]");
        }

        [Fact]
        public void ShouldTranslateAStringConstantConstructorParameter()
        {
            var createStringBuilder = CreateLambda(() => new StringBuilder("Hello!"));

            var translated = createStringBuilder.Body.ToReadableString();

            translated.ShouldBe("new StringBuilder(\"Hello!\")");
        }

        [Fact]
        public void ShouldTranslateACharacterConstantConstructorParameter()
        {
            var createCharCtor = CreateLambda(() => new CharCtor('f'));

            var translated = createCharCtor.Body.ToReadableString();

            translated.ShouldBe("new CharCtor('f')");
        }

        [Fact]
        public void ShouldTranslateConstantConstructorParameters()
        {
            var createStringBuilder = CreateLambda(() => new StringBuilder(1000, 10000));

            var translated = createStringBuilder.Body.ToReadableString();

            translated.ShouldBe("new StringBuilder(1000, 10000)");
        }

        [Fact]
        public void ShouldTranslateAParameterConstructorParameter()
        {
            var createStringBuilder = CreateLambda((string str) => new StringBuilder(str));

            var translated = createStringBuilder.Body.ToReadableString();

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

            var translated = createStringBuilder.ToReadableString();

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

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnAnonymousTypeCreation()
        {
            var anonType = new { ValueInt = default(int) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(int));

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = New(constructor, Constant(100));

            var translated = creation.ToReadableString();

            translated.ShouldBe("new { ValueInt = 100 }");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/33
        [Fact]
        public void ShouldTranslateAnAnonymousTypeCreationWithACustomFactory()
        {
            var anonType = new { ValueInt = default(int) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(int));

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = New(constructor, Constant(10));

            var translated = creation.ToReadableString(stgs => stgs
                .NameAnonymousTypesUsing(_ => "MyMagicObject"));

            translated.ShouldBe("new MyMagicObject { ValueInt = 10 }");
        }

        [Fact]
        public void ShouldTranslateAFullyQualfiedAnonymousTypeCreation()
        {
            var anonType = new { Value = default(TimeSpan) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(TimeSpan));

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = New(constructor, Default(typeof(TimeSpan)));

            var translated = creation.ToReadableString(stgs => stgs.UseFullyQualifiedTypeNames);

            translated.ShouldBe("new { Value = default(System.TimeSpan) }");
        }

#if FEATURE_VALUE_TUPLE
        [Fact]
        public void ShouldTranslateAValueTupleNewExpression()
        {
            var valueTuple = (default(int), default(string));
            var valueTupleType = valueTuple.GetType();

            var valueTupleCtor = valueTupleType
                .GetPublicInstanceConstructor(typeof(int), typeof(string));

            var stringEmpty = typeof(string)
                .GetPublicStaticField(nameof(string.Empty));

            var newValueTuple = New(
                valueTupleCtor,
                Constant(123),
                Field(null, stringEmpty));

            var tupleVariable = Variable(valueTupleType);
            var assignTuple = Assign(tupleVariable, newValueTuple);

            var translated = assignTuple.ToReadableString();

            translated.ShouldBe("intStringValueTuple = (123, string.Empty)");
        }
#endif
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

    internal class CharCtor
    {
        public CharCtor(char value)
        {
            Value = value;
        }

        public char Value { get; }
    }

    #endregion
}

public class NoNamespace { }