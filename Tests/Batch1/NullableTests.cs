﻿using Bridge.Test;
using System;

#pragma warning disable 219

namespace Bridge.ClientTest
{
    [Category(Constants.MODULE_NULLABLE)]
    [TestFixture(TestNameFormat = "Nullable - {0}")]
    public class NullableTests
    {
        private bool IsOfType<T>(object value)
        {
            return value is T;
        }

        [Test]
        public void TypePropertiesAreCorrect()
        {
            int? a = 3, b = null;
            Assert.AreEqual("System.Nullable$1[[Boolean]]", typeof(Nullable<bool>).FullName, "Open FullName");
            Assert.AreEqual("System.Nullable$1[[System.Int32, mscorlib]]", typeof(int?).FullName, "Instantiated FullName");
            Assert.True((object)a is int?, "is int? #1");
            Assert.False((object)b is int?, "is int? #2");

            Assert.True(IsOfType<int?>(3), "IsOfType #1");
            Assert.False(IsOfType<int?>(3.14), "IsOfType #2");
            Assert.True(IsOfType<TimeSpan?>(new TimeSpan(1)), "IsOfType #3");
            Assert.False(IsOfType<TimeSpan?>(3.14), "IsOfType #4");
        }

        [Test]
        public void ConvertingToNullableWorks()
        {
            int i = 3;
            int? i1 = new int?(i);
            int? i2 = i;
            Assert.AreEqual(3, i1);
            Assert.AreEqual(3, i2);
        }

        [Test]
        public void HasValueWorks()
        {
            int? a = 3, b = null;
            Assert.True(a.HasValue);
            Assert.False(b.HasValue);
        }

        [Test]
        public void BoxingWorks()
        {
            int? a = 3, b = null;
            Assert.True((object)a != null);
            Assert.False((object)b != null);
        }

        [Test]
        public void UnboxingWorks()
        {
            int? a = 3, b = null;
            Assert.AreEqual(3, (int)a);
            try
            {
                int x = (int)b;
                Assert.Fail("Unboxing null should have thrown an exception");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Test]
        public void ValueWorks()
        {
            int? a = 3, b = null;
            Assert.AreEqual(3, a.Value);
            try
            {
                int x = b.Value;
                Assert.Fail("null.Value should have thrown an exception");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Test]
        public void UnboxingValueOfWrongTypeThrowsAnException()
        {
            Assert.Throws(() =>
            {
                object o = "x";
                int x = (int)o;
            });
        }

        [Test]
        public void GetValueOrDefaultWithArgWorks()
        {
            int? a = 3, b = null;
            Assert.AreEqual(3, a.GetValueOrDefault(1));
            Assert.AreEqual(1, b.GetValueOrDefault(1));
        }

        [Test]
        public void LiftedEqualityWorks()
        {
            int? a = 1, b = 1, c = 2, d = null, e = null;
            Assert.AreStrictEqual(true, a == b);
            Assert.AreStrictEqual(false, a == c);
            Assert.AreStrictEqual(false, a == d);
            Assert.AreStrictEqual(true, d == e);
        }

        [Test]
        public void LiftedInequalityWorks()
        {
            int? a = 1, b = 1, c = 2, d = null, e = null;
            Assert.AreStrictEqual(false, a != b);
            Assert.AreStrictEqual(true, a != c);
            Assert.AreStrictEqual(true, a != d);
            Assert.AreStrictEqual(false, d != e);
        }

        [Test]
        public void LiftedLessThanWorks()
        {
            int? a = 1, b = 1, c = 2, d = null, e = null;
            Assert.AreStrictEqual(false, a < b);
            Assert.AreStrictEqual(true, a < c);
            Assert.AreStrictEqual(false, a < d);
            Assert.AreStrictEqual(false, d < e);
        }

        [Test]
        public void LiftedGreaterThanWorks()
        {
            int? a = 1, b = 1, c = 2, d = null, e = null;
            Assert.AreStrictEqual(false, a > b);
            Assert.AreStrictEqual(true, c > a);
            Assert.AreStrictEqual(false, a > d);
            Assert.AreStrictEqual(false, d > e);
        }

        [Test]
        public void LiftedLessThanOrEqualWorks()
        {
            int? a = 1, b = 1, c = 2, d = null, e = null;
            Assert.AreStrictEqual(true, a <= b);
            Assert.AreStrictEqual(false, c <= a);
            Assert.AreStrictEqual(false, a <= d);
            Assert.AreStrictEqual(false, d <= e);
        }

        [Test]
        public void LiftedGreaterThanOrEqualWorks()
        {
            int? a = 1, b = 1, c = 2, d = null, e = null;
            Assert.AreStrictEqual(true, a >= b);
            Assert.AreStrictEqual(false, a >= c);
            Assert.AreStrictEqual(false, a >= d);
            Assert.AreStrictEqual(false, d >= e);
        }

        [Test]
        public void LiftedSubtractionWorks()
        {
            int? a = 2, b = 3, c = null;
            Assert.AreStrictEqual(-1, a - b);
            Assert.AreStrictEqual(null, a - c);
        }

        [Test]
        public void LiftedAdditionWorks()
        {
            int? a = 2, b = 3, c = null;
            Assert.AreStrictEqual(5, a + b);
            Assert.AreStrictEqual(null, a + c);
        }

        [Test]
        public void LiftedModWorks()
        {
            int? a = 14, b = 3, c = null;
            Assert.AreStrictEqual(2, a % b);
            Assert.AreStrictEqual(null, a % c);
        }

        [Test]
        public void LiftedFloatingPointDivisionWorks()
        {
            double? a = 15, b = 3, c = null;
            Assert.AreStrictEqual(5, a / b);
            Assert.AreStrictEqual(null, a / c);
        }

        [Test]
        public void LiftedIntegerDivisionWorks()
        {
            int? a = 16, b = 3, c = null;
            Assert.AreStrictEqual(5, a / b);
            Assert.AreStrictEqual(null, a / c);
        }

        [Test]
        public void LiftedMultiplicationWorks()
        {
            int? a = 2, b = 3, c = null;
            Assert.AreStrictEqual(6, a * b);
            Assert.AreStrictEqual(null, a * c);
        }

        [Test]
        public void LiftedBitwiseAndWorks()
        {
            int? a = 6, b = 3, c = null;
            Assert.AreStrictEqual(2, a & b);
            Assert.AreStrictEqual(null, a & c);
        }

        [Test]
        public void LiftedBitwiseOrWorks()
        {
            int? a = 6, b = 3, c = null;
            Assert.AreStrictEqual(7, a | b);
            Assert.AreStrictEqual(null, a | c);
        }

        [Test]
        public void LiftedBitwiseXorWorks()
        {
            int? a = 6, b = 3, c = null;
            Assert.AreStrictEqual(5, a ^ b);
            Assert.AreStrictEqual(null, a ^ c);
        }

        [Test]
        public void LiftedLeftShiftWorks()
        {
            int? a = 6, b = 3, c = null;
            Assert.AreStrictEqual(48, a << b);
            Assert.AreStrictEqual(null, a << c);
        }

        [Test]
        public void LiftedSignedRightShiftWorks()
        {
            int? a = 48, b = 3, c = null;
            Assert.AreStrictEqual(6, a >> b);
            Assert.AreStrictEqual(null, a >> c);
        }

        [Test]
        public void LiftedUnsignedRightShiftWorks()
        {
            int? a = -48, b = 3, c = null;
            Assert.AreStrictEqual(-6, a >> b);
            Assert.AreStrictEqual(null, a >> c);
        }

        [Test(Name = "{0} #314")]
        public void LiftedBooleanAndWorks()
        {
            bool? a = true, b = true, c = false, d = false, e = null, f = null;
            Assert.AreStrictEqual(true, a & b);
            Assert.AreStrictEqual(false, a & c);
            Assert.AreStrictEqual(null, a & e);
            Assert.AreStrictEqual(false, c & a);
            Assert.AreStrictEqual(false, c & d);
            Assert.AreStrictEqual(false, c & e);
            Assert.AreStrictEqual(null, e & a);
            Assert.AreStrictEqual(false, e & c);
            Assert.AreStrictEqual(null, e & f);
        }

        [Test(Name = "{0} #314")]
        public void LiftedBooleanOrWorks()
        {
            bool? a = true, b = true, c = false, d = false, e = null, f = null;
            Assert.AreStrictEqual(true, a | b);
            Assert.AreStrictEqual(true, a | c);
            Assert.AreStrictEqual(true, a | e);
            Assert.AreStrictEqual(true, c | a);
            Assert.AreStrictEqual(false, c | d);
            Assert.AreStrictEqual(null, c | e);
            Assert.AreStrictEqual(true, e | a);
            Assert.AreStrictEqual(null, e | c);
            Assert.AreStrictEqual(null, e | f);
        }

        [Test]
        public void LiftedBooleanNotWorks()
        {
            bool? a = true, b = false, c = null;
            Assert.AreStrictEqual(false, !a);
            Assert.AreStrictEqual(true, !b);
            Assert.AreStrictEqual(null, !c);
        }

        [Test]
        public void LiftedNegationWorks()
        {
            int? a = 3, b = null;
            Assert.AreStrictEqual(-3, -a);
            Assert.AreStrictEqual(null, -b);
        }

        [Test]
        public void LiftedUnaryPlusWorks()
        {
            int? a = 3, b = null;
            Assert.AreStrictEqual(+3, +a);
            Assert.AreStrictEqual(null, +b);
        }

        [Test]
        public void LiftedOnesComplementWorks()
        {
            int? a = 3, b = null;
            Assert.AreStrictEqual(-4, ~a);
            Assert.AreStrictEqual(null, ~b);
        }

        [Test(Name = "{0} #314")]
        public void CoalesceWorks()
        {
            int? v1 = null, v2 = 1, v3 = 0, v4 = 2;
            string s1 = null, s2 = "x";
            Assert.AreStrictEqual(null, v1 ?? v1);
            Assert.AreStrictEqual(1, v1 ?? v2);
            Assert.AreStrictEqual(0, v3 ?? v4);
            Assert.AreStrictEqual(null, s1 ?? s1);
            Assert.AreStrictEqual("x", s1 ?? s2);
        }
    }
}