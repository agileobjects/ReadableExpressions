namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingLogicalOperations
    {
        [TestMethod]
        public void ShouldTranslateAnAndOperation()
        {
            Expression<Func<bool, bool, bool>> bothBoolsAreTheSame = (b1, b2) => b1 && b2;

            var translated = bothBoolsAreTheSame.Body.ToReadableString();

            Assert.AreEqual("(b1 && b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseAndOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseAnd = (b1, b2) => b1 & b2;

            var translated = bitwiseAnd.Body.ToReadableString();

            Assert.AreEqual("(b1 & b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnOrOperation()
        {
            Expression<Func<bool, bool, bool>> eitherBoolsIsTrue = (b1, b2) => b1 || b2;

            var translated = eitherBoolsIsTrue.Body.ToReadableString();

            Assert.AreEqual("(b1 || b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseOrOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseOr = (b1, b2) => b1 | b2;

            var translated = bitwiseOr.Body.ToReadableString();

            Assert.AreEqual("(b1 | b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseExclusiveOrOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseExclusiveOr = (b1, b2) => b1 ^ b2;

            var translated = bitwiseExclusiveOr.Body.ToReadableString();

            Assert.AreEqual("(b1 ^ b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseLeftShiftOperation()
        {
            Expression<Func<int, int, int>> bitwiseLeftShift = (i1, i2) => i1 << i2;

            var translated = bitwiseLeftShift.Body.ToReadableString();

            Assert.AreEqual("(i1 << i2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseRightShiftOperation()
        {
            Expression<Func<int, int, int>> bitwiseRightShift = (i1, i2) => i1 >> i2;

            var translated = bitwiseRightShift.Body.ToReadableString();

            Assert.AreEqual("(i1 >> i2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAUnaryPlusOperation()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var unaryPlus = Expression.UnaryPlus(intVariable);

            var translated = unaryPlus.ToReadableString();

            Assert.AreEqual("+i", translated);
        }

        [TestMethod]
        public void ShouldTranslateAOnesComplementOperation()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var onesComplement = Expression.OnesComplement(intVariable);

            var translated = onesComplement.ToReadableString();

            Assert.AreEqual("~i", translated);
        }

        [TestMethod]
        public void ShouldTranslateACoalesceOperation()
        {
            Expression<Func<bool?, bool, bool>> oneOrTwo = (b1, b2) => b1 ?? b2;

            var translated = oneOrTwo.Body.ToReadableString();

            Assert.AreEqual("(b1 ?? b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAConditionalOperation()
        {
            Expression<Func<int, string>> whatSize = i => (i < 8) ? "Too small" : "Too big";

            var translated = whatSize.Body.ToReadableString();

            Assert.AreEqual("(i < 8) ? \"Too small\" : \"Too big\"", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIsTypeExpression()
        {
            Expression<Func<object, bool>> objectIsDisposable = o => o is IDisposable;

            var translated = objectIsDisposable.Body.ToReadableString();

            Assert.AreEqual("(o is IDisposable)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAValueTypeTypeEqualExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intIsLong = Expression.TypeEqual(intVariable, typeof(long));

            var translated = intIsLong.ToReadableString();

            Assert.AreEqual("false", translated);
        }

        [TestMethod]
        public void ShouldTranslateANullableValueTypeTypeEqualExpression()
        {
            var nullableLongVariable = Expression.Variable(typeof(long?), "l");
            var nullableLongIsNullableLong = Expression.TypeEqual(nullableLongVariable, typeof(long?));

            var translated = nullableLongIsNullableLong.ToReadableString();

            Assert.AreEqual("(l != null)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAConstantTypeEqualExpression()
        {
            var intConstant = Expression.Constant(123, typeof(int));
            var intConstantIsInt = Expression.TypeEqual(intConstant, typeof(int));

            var translated = intConstantIsInt.ToReadableString();

            Assert.AreEqual("true", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnObjectTypeEqualExpression()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectIsString = Expression.TypeEqual(objectVariable, typeof(string));

            var translated = objectIsString.ToReadableString();

            Assert.AreEqual("((o != null) && (o.GetType() == typeof(string)))", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIsTrueExpression()
        {
            var boolVariable = Expression.Variable(typeof(bool), "b");
            var boolIsTrue = Expression.IsTrue(boolVariable);

            var translated = boolIsTrue.ToReadableString();

            Assert.AreEqual("b", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIsFalseExpression()
        {
            var boolVariable = Expression.Variable(typeof(bool), "b");
            var boolIsFalse = Expression.IsFalse(boolVariable);

            var translated = boolIsFalse.ToReadableString();

            Assert.AreEqual("!b", translated);
        }
    }
}