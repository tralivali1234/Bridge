using Bridge.Test;
using Bridge.Utils;
using System;
using System.Collections.Generic;

namespace Bridge.ClientTest.Reflection
{
    [Category(Constants.MODULE_REFLECTION)]
    [TestFixture(TestNameFormat = "Reflection - TypeSystem {0}")]
    public class TypeSystemTests
    {
        public class ClassWithExpandParamsCtor
        {
            public object[] CtorArgs;

            [ExpandParams]
            public ClassWithExpandParamsCtor(params object[] args)
            {
                this.CtorArgs = args;
            }
        }

        public interface I1 { }

        public interface I2 : I1 { }

        public interface I3 { }

        public interface I4 : I3 { }

        public class B : I2 { }

        public class C : B, I4 { }

        public interface IG<T> { }

        public class BX<T> { }

        public class G<T1, T2> : BX<G<T1, C>>, IG<G<T2, string>>
        {
            public static string field;

            static G()
            {
                field = typeof(T1).FullName + " " + typeof(T2).FullName;
            }
        }

        public enum E1 { }

        [Flags]
        public enum E2 { }

        [External]
        [Name("Object")]
        public interface IImported { }

        public class BS
        {
            [Name(false)]
            public int X;

            public BS(int x)
            {
                X = x;
            }
        }

        public class DS : BS
        {
            public int GetX()
            {
                return X;
            }

            public DS(int x) : base(x)
            {
            }
        }

        public class CS2
        {
            public int X;
        }

        public class DS2 : BS
        {
            public DS2() : base(0)
            {
            }
        }

        private string AssemblyName
        {
            get
            {
                return "Bridge.ClientTest";
            }
        }

        private string AssemblyWithVersion
        {
            get
            {
                //return AssemblyName + ", Version=" + AssemblyVersionMarker.GetVersion(AssemblyVersionMarker.VersionType.CurrentAssembly);
                return AssemblyName;
            }
        }

        [Test]
        public void FullNamePropertyReturnsTheNameWithTheNamespace()
        {
            Assert.AreEqual(typeof(TypeSystemTests).FullName, "Bridge.ClientTest.Reflection.TypeSystemTests");
        }

        [Test]
        public void AssemblyQualifiedNameReturnsTheNameWithTheNamespaceAndAssemblyName()
        {
            Assert.AreEqual(typeof(TypeSystemTests).AssemblyQualifiedName, "Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest");
            Assert.AreEqual(typeof(BX<>).AssemblyQualifiedName, "Bridge.ClientTest.Reflection.TypeSystemTests.BX$1, Bridge.ClientTest");
            Assert.AreEqual(typeof(BX<int>).AssemblyQualifiedName, "Bridge.ClientTest.Reflection.TypeSystemTests.BX$1[[System.Int32, mscorlib]], Bridge.ClientTest");
        }

        [Test]
        public void AssemblyPropertyWorks()
        {
            Assert.AreEqual(typeof(B).Assembly.FullName, AssemblyWithVersion);
            Assert.AreEqual(typeof(I1).Assembly.FullName, AssemblyWithVersion);
            Assert.AreEqual(typeof(IG<>).Assembly.FullName, AssemblyWithVersion);
            Assert.AreEqual(typeof(BX<>).Assembly.FullName, AssemblyWithVersion);
            Assert.AreEqual(typeof(IG<int>).Assembly.FullName, AssemblyWithVersion);
            Assert.AreEqual(typeof(BX<int>).Assembly.FullName, AssemblyWithVersion);
            Assert.AreEqual(typeof(E1).Assembly.FullName, AssemblyWithVersion);
        }

        [Test]
        public void NamespacePropertyReturnsTheNamespaceWithoutTheName()
        {
            Assert.AreEqual(typeof(TypeSystemTests).Namespace, "Bridge.ClientTest.Reflection");
            Assert.AreEqual(typeof(DS2).Namespace, "Bridge.ClientTest.Reflection");
        }

        [Test]
        public void InstantiatingClassWithConstructorThatNeedsToBeAppliedWorks()
        {
            var args = new List<object> { 42, "x", 18 };
            var obj = new ClassWithExpandParamsCtor(args.ToArray());

            Assert.AreEqual(obj.CtorArgs, args.ToArray());
            Assert.AreEqual(obj.GetType(), typeof(ClassWithExpandParamsCtor));
        }

        [Test]
        public void NamePropertyRemovesTheNamespace()
        {
            Assert.AreEqual(typeof(TypeSystemTests).Name, "TypeSystemTests", "non-generic");
            Assert.AreEqual(typeof(G<int, string>).Name, "G$2[[System.Int32, mscorlib],[String]]", "generic");
            Assert.AreEqual(typeof(G<BX<double>, string>).Name, "G$2[[Bridge.ClientTest.Reflection.TypeSystemTests.BX$1[[System.Double, mscorlib]], Bridge.ClientTest],[String]]", "nested generic");
        }

        [Test]
        public void GettingBaseTypeWorks()
        {
            Assert.AreEqual(typeof(B).BaseType, typeof(object));
            Assert.AreEqual(typeof(C).BaseType, typeof(B));
            Assert.AreEqual(typeof(object).BaseType, null);
        }

        [Test]
        public void GettingImplementedInterfacesWorks()
        {
            var ifs = typeof(C).GetInterfaces();
            Assert.AreEqual(ifs.Length, 4);
            Assert.True(ifs.Contains(typeof(I1)));
            Assert.True(ifs.Contains(typeof(I2)));
            Assert.True(ifs.Contains(typeof(I3)));
            Assert.True(ifs.Contains(typeof(I4)));
        }

        [Test]
        public void TypeOfAnOpenGenericClassWorks()
        {
            Assert.AreEqual(typeof(G<,>).FullName, "Bridge.ClientTest.Reflection.TypeSystemTests.G$2");
        }

        [Test]
        public void TypeOfAnOpenGenericInterfaceWorks()
        {
            Assert.AreEqual(typeof(IG<>).FullName, "Bridge.ClientTest.Reflection.TypeSystemTests.IG$1");
        }

        [Test]
        public void TypeOfInstantiatedGenericClassWorks()
        {
            Assert.AreEqual(typeof(G<int, C>).FullName, "Bridge.ClientTest.Reflection.TypeSystemTests.G$2[[System.Int32, mscorlib],[Bridge.ClientTest.Reflection.TypeSystemTests.C, Bridge.ClientTest]]");
        }

        [Test]
        public void TypeOfInstantiatedGenericInterfaceWorks()
        {
            Assert.AreEqual(typeof(IG<int>).FullName, "Bridge.ClientTest.Reflection.TypeSystemTests.IG$1[[System.Int32, mscorlib]]");
        }

        [Test]
        public void ConstructingAGenericTypeTwiceWithTheSameArgumentsReturnsTheSameInstance()
        {
            var t1 = typeof(G<int, C>);
            var t2 = typeof(G<C, int>);
            var t3 = typeof(G<int, C>);
            Assert.False(t1 == t2);
            Assert.True(t1 == t3);
        }

        [Test]
        public void AccessingAStaticMemberInAGenericClassWorks()
        {
            Assert.AreEqual(G<int, C>.field, "System.Int32 Bridge.ClientTest.Reflection.TypeSystemTests.C");
            Assert.AreEqual(G<C, int>.field, "Bridge.ClientTest.Reflection.TypeSystemTests.C System.Int32");
            Assert.AreEqual(G<G<C, int>, G<string, C>>.field, "Bridge.ClientTest.Reflection.TypeSystemTests.G$2[[Bridge.ClientTest.Reflection.TypeSystemTests.C, Bridge.ClientTest],[System.Int32, mscorlib]] Bridge.ClientTest.Reflection.TypeSystemTests.G$2[[String],[Bridge.ClientTest.Reflection.TypeSystemTests.C, Bridge.ClientTest]]");
        }

        [Test]
        public void TypeOfNestedGenericClassWorks()
        {
            Assert.AreEqual(typeof(G<int, G<C, IG<string>>>).FullName, "Bridge.ClientTest.Reflection.TypeSystemTests.G$2[[System.Int32, mscorlib],[Bridge.ClientTest.Reflection.TypeSystemTests.G$2[[Bridge.ClientTest.Reflection.TypeSystemTests.C, Bridge.ClientTest],[Bridge.ClientTest.Reflection.TypeSystemTests.IG$1[[String]], Bridge.ClientTest]], Bridge.ClientTest]]");
        }

        [Test]
        public void BaseTypeAndImplementedInterfacesForGenericTypeWorks()
        {
            Assert.AreEqual(typeof(G<int, G<C, IG<string>>>).BaseType.FullName, "Bridge.ClientTest.Reflection.TypeSystemTests.BX$1[[Bridge.ClientTest.Reflection.TypeSystemTests.G$2[[System.Int32, mscorlib],[Bridge.ClientTest.Reflection.TypeSystemTests.C, Bridge.ClientTest]], Bridge.ClientTest]]");
            Assert.AreEqual(typeof(G<int, G<C, IG<string>>>).GetInterfaces()[0].FullName, "Bridge.ClientTest.Reflection.TypeSystemTests.IG$1[[Bridge.ClientTest.Reflection.TypeSystemTests.G$2[[Bridge.ClientTest.Reflection.TypeSystemTests.G$2[[Bridge.ClientTest.Reflection.TypeSystemTests.C, Bridge.ClientTest],[Bridge.ClientTest.Reflection.TypeSystemTests.IG$1[[String]], Bridge.ClientTest]], Bridge.ClientTest],[String]], Bridge.ClientTest]]");
        }

        [Test]
        public void IsGenericTypeDefinitionWorksAsExpected()
        {
            Assert.True(typeof(G<,>).IsGenericTypeDefinition);
            Assert.False(typeof(G<int, string>).IsGenericTypeDefinition);
            Assert.False(typeof(C).IsGenericTypeDefinition);
            Assert.True(typeof(IG<>).IsGenericTypeDefinition);
            Assert.False(typeof(IG<int>).IsGenericTypeDefinition);
            Assert.False(typeof(I2).IsGenericTypeDefinition);
            Assert.False(typeof(E1).IsGenericTypeDefinition);
        }

        [Test]
        public void GenericParameterCountReturnsZeroForConstructedTypesAndNonZeroForOpenOnes()
        {
            Assert.AreEqual(typeof(G<,>).GenericParameterCount, 2);
            Assert.AreEqual(typeof(G<int, string>).GenericParameterCount, 0);
            Assert.AreEqual(typeof(C).GenericParameterCount, 0);
            Assert.AreEqual(typeof(IG<>).GenericParameterCount, 1);
            Assert.AreEqual(typeof(IG<int>).GenericParameterCount, 0);
            Assert.AreEqual(typeof(I2).GenericParameterCount, 0);
            Assert.AreEqual(typeof(E1).GenericParameterCount, 0);
        }

        [Test]
        public void GetGenericArgumentsReturnsTheCorrectTypesForConstructedTypesOtherwiseNull()
        {
            Assert.AreEqual(typeof(G<,>).GetGenericArguments(), null);
            Assert.AreEqual(typeof(G<int, string>).GetGenericArguments(), new[] { typeof(int), typeof(string) });
            Assert.AreEqual(typeof(C).GetGenericArguments(), null);
            Assert.AreEqual(typeof(IG<>).GetGenericArguments(), null);
            Assert.AreEqual(typeof(IG<string>).GetGenericArguments(), new[] { typeof(string) });
            Assert.AreEqual(typeof(I2).GetGenericArguments(), null);
            Assert.AreEqual(typeof(E1).GetGenericArguments(), null);
        }

        [Test]
        public void GetGenericTypeDefinitionReturnsTheGenericTypeDefinitionForConstructedTypeOtherwiseNull()
        {
            Assert.AreEqual(typeof(G<,>).GetGenericTypeDefinition(), typeof(G<,>));
            Assert.AreEqual(typeof(G<int, string>).GetGenericTypeDefinition(), typeof(G<,>));
            Assert.Throws<InvalidOperationException>(() => typeof(C).GetGenericTypeDefinition());
            Assert.AreEqual(typeof(IG<>).GetGenericTypeDefinition(), typeof(IG<>));
            Assert.AreEqual(typeof(IG<string>).GetGenericTypeDefinition(), typeof(IG<>));
            Assert.Throws<InvalidOperationException>(() => typeof(I2).GetGenericTypeDefinition());
            Assert.Throws<InvalidOperationException>(() => typeof(E1).GetGenericTypeDefinition());
        }

        private class IsAssignableFromTypes
        {
            public class C1 { }

            public class C2<T> { }

            public interface I1 { }

            public interface I2<T1> { }

            public interface I3 : I1 { }

            public interface I4 { }

            public interface I5<T1> : I2<T1> { }

            public interface I6<out T> { }

            public interface I7<in T> { }

            public interface I8<out T1, in T2> : I6<T1>, I7<T2> { }

            public interface I9<T1, out T2> { }

            public interface I10<out T1, in T2> : I8<T1, T2> { }

            public class D1 : C1, I1 { }

            public class D2<T> : C2<T>, I2<T>, I1
            {
            }

            public class D3 : C2<int>, I2<string>
            {
            }

            public class D4 : I3, I4
            {
            }

            public class X1 : I1
            {
            }

            public class X2 : X1
            {
            }

            public class Y1<T> : I6<T> { }

            public class Y1X1 : Y1<X1> { }

            public class Y1X2 : Y1<X2> { }

            public class Y2<T> : I7<T> { }

            public class Y2X1 : Y2<X1> { }

            public class Y2X2 : Y2<X2> { }

            public class Y3<T1, T2> : I8<T1, T2> { }

            public class Y3X1X1 : Y3<X1, X1> { }

            public class Y3X1X2 : Y3<X1, X2> { }

            public class Y3X2X1 : Y3<X2, X1> { }

            public class Y3X2X2 : Y3<X2, X2> { }

            public class Y4<T1, T2> : I9<T1, T2> { }

            public class Y5<T1, T2> : I6<I8<T1, T2>> { }

            public class Y6<T1, T2> : I7<I8<T1, T2>> { }
        }

        [Test]
        public void IsAssignableFromWorks()
        {
            Assert.True(typeof(IsAssignableFromTypes.C1).IsAssignableFrom(typeof(IsAssignableFromTypes.C1)), "#1");
            Assert.False(typeof(IsAssignableFromTypes.C1).IsAssignableFrom(typeof(object)), "#2");
            Assert.True(typeof(object).IsAssignableFrom(typeof(IsAssignableFromTypes.C1)), "#3");
            Assert.False(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(object)), "#4");
            Assert.True(typeof(object).IsAssignableFrom(typeof(IsAssignableFromTypes.I1)), "#5");
            Assert.False(typeof(IsAssignableFromTypes.I3).IsAssignableFrom(typeof(IsAssignableFromTypes.I1)), "#6");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.I3)), "#7");
            Assert.False(typeof(IsAssignableFromTypes.D1).IsAssignableFrom(typeof(IsAssignableFromTypes.C1)), "#8");
            Assert.True(typeof(IsAssignableFromTypes.C1).IsAssignableFrom(typeof(IsAssignableFromTypes.D1)), "#9");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D1)), "#10");
            Assert.True(typeof(IsAssignableFromTypes.C2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#11");
            Assert.False(typeof(IsAssignableFromTypes.C2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#12");
            Assert.True(typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#13");
            Assert.False(typeof(IsAssignableFromTypes.I2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#14");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#15");
            Assert.False(typeof(IsAssignableFromTypes.C2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#16");
            Assert.True(typeof(IsAssignableFromTypes.C2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#17");
            Assert.False(typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#18");
            Assert.True(typeof(IsAssignableFromTypes.I2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#19");
            Assert.False(typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<string>)), "#20");
            Assert.True(typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<int>)), "#21");
            Assert.False(typeof(IsAssignableFromTypes.I5<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I2<int>)), "#22");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)), "#23");
            Assert.True(typeof(IsAssignableFromTypes.I3).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)), "#24");
            Assert.True(typeof(IsAssignableFromTypes.I4).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)), "#25");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.X2)), "#26");
            Assert.False(typeof(IsAssignableFromTypes.I2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<>)), "#27");
            Assert.False(typeof(IsAssignableFromTypes.C2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<>)), "#28");
            Assert.False(typeof(IsAssignableFromTypes.C2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#29");
            Assert.False(typeof(E1).IsAssignableFrom(typeof(E2)), "#30");
            Assert.False(typeof(int).IsAssignableFrom(typeof(E1)), "#31");
            Assert.True(typeof(object).IsAssignableFrom(typeof(E1)), "#32");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>)), "#33");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>)), "#34");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>)), "#35");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>)), "#36");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>)), "#37");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>)), "#38");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>)), "#39");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1X1)), "#40");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>)), "#41");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1X1)), "#42");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X2>)), "#43");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1X2)), "#44");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X2>)), "#45");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1X2)), "#46");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>)), "#47");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>)), "#48");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>)), "#49");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>)), "#50");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>)), "#51");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>)), "#52");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>)), "#53");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2X1)), "#54");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>)), "#55");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2X1)), "#56");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X2>)), "#57");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2X2)), "#58");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X2>)), "#59");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2X2)), "#60");
            Assert.False(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#61");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#62");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#63");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#64");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#65");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#66");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#67");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#68");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#69");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#70");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#71");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#72");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#73");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#74");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#75");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#76");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#77");
            Assert.False(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#78");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#79");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X1)), "#80");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#81");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X2)), "#82");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#83");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X1)), "#84");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#85");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X2)), "#86");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#87");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X1)), "#88");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#89");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X2)), "#90");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#91");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X1)), "#92");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#93");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X2)), "#94");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#95");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X1)), "#96");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#97");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X2)), "#98");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#99");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X1)), "#100");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#101");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X2)), "#102");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#103");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X1)), "#104");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#105");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X2)), "#106");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#107");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X1)), "#108");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#109");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X2)), "#110");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>)), "#111");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>)), "#112");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>)), "#113");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>)), "#114");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>)), "#115");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>)), "#116");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>)), "#117");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>)), "#118");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>)), "#119");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>)), "#120");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>)), "#121");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>)), "#122");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>)), "#123");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>)), "#124");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>)), "#125");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>)), "#126");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>)), "#127");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>)), "#128");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>)), "#129");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>)), "#130");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>)), "#131");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>)), "#132");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>)), "#133");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>)), "#134");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>)), "#135");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>)), "#136");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>)), "#137");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>)), "#138");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>)), "#139");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>)), "#140");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>)), "#141");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>)), "#142");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#143");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#144");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#145");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#146");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#147");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#148");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#149");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#150");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#151");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#152");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#153");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#154");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#155");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#156");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#157");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#158");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#159");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#160");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#161");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#162");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#163");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#164");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#165");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#166");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#167");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#168");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#169");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#170");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#171");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#172");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#173");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#174");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#175");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#176");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#177");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#178");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#179");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#180");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#181");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#182");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#183");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#184");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#185");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#186");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#187");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#188");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#189");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#190");
        }

        private class IsSubclassOfTypes
        {
            public class C1 { }

            public class C2<T> { }

            public class D1 : C1 { }

            public class D2<T> : C2<T> { }

            public class D3 : C2<int> { }
        }

        [Test]
        public void IsSubclassOfWorks()
        {
            Assert.False(typeof(IsSubclassOfTypes.C1).IsSubclassOf(typeof(IsSubclassOfTypes.C1)), "#1");
            Assert.True(typeof(IsSubclassOfTypes.C1).IsSubclassOf(typeof(object)), "#2");
            Assert.False(typeof(object).IsSubclassOf(typeof(IsSubclassOfTypes.C1)), "#3");
            Assert.True(typeof(IsSubclassOfTypes.D1).IsSubclassOf(typeof(IsSubclassOfTypes.C1)), "#4");
            Assert.False(typeof(IsSubclassOfTypes.C1).IsSubclassOf(typeof(IsSubclassOfTypes.D1)), "#5");
            Assert.True(typeof(IsSubclassOfTypes.D1).IsSubclassOf(typeof(object)), "#6");
            Assert.True(typeof(IsSubclassOfTypes.D2<int>).IsSubclassOf(typeof(IsSubclassOfTypes.C2<int>)), "#7");
            Assert.False(typeof(IsSubclassOfTypes.D2<string>).IsSubclassOf(typeof(IsSubclassOfTypes.C2<int>)), "#8");
            Assert.False(typeof(IsSubclassOfTypes.D3).IsSubclassOf(typeof(IsSubclassOfTypes.C2<string>)), "#9");
            Assert.True(typeof(IsSubclassOfTypes.D3).IsSubclassOf(typeof(IsSubclassOfTypes.C2<int>)), "#10");
            Assert.False(typeof(IsSubclassOfTypes.D2<>).IsSubclassOf(typeof(IsSubclassOfTypes.C2<>)), "#11");
            Assert.False(typeof(IsSubclassOfTypes.D3).IsSubclassOf(typeof(IsSubclassOfTypes.C2<>)), "#12");
        }

        [Test]
        public void IsClassWorks()
        {
            Assert.False(typeof(E1).IsClass);
            Assert.False(typeof(E2).IsClass);
            Assert.True(typeof(C).IsClass);
            Assert.True(typeof(G<,>).IsClass);
            Assert.True(typeof(G<int, string>).IsClass);
            Assert.False(typeof(I1).IsClass);
            Assert.False(typeof(IG<>).IsClass);
            Assert.False(typeof(IG<int>).IsClass);
        }

        [Test]
        public void IsEnumWorks()
        {
            Assert.True(typeof(E1).IsEnum);
            Assert.True(typeof(E2).IsEnum);
            Assert.False(typeof(C).IsEnum);
            Assert.False(typeof(G<,>).IsEnum);
            Assert.False(typeof(G<int, string>).IsEnum);
            Assert.False(typeof(I1).IsEnum);
            Assert.False(typeof(IG<>).IsEnum);
            Assert.False(typeof(IG<int>).IsEnum);
        }

        [Test]
        public void IsArrayWorks()
        {
            var array = new int[5];
            Assert.True(array.GetType().IsArray);
            Assert.True(typeof(object[]).IsArray);
            Assert.True(typeof(int[]).IsArray);
            Assert.False(typeof(C).IsArray);
            //TODO Assert.False(typeof(List<int>).IsArray);
            //TODO Assert.False(typeof(Array).IsArray);
        }

        [Test]
        public void IsFlagsWorks()
        {
            Assert.False(typeof(E1).IsFlags);
            Assert.True(typeof(E2).IsFlags);
            Assert.False(typeof(C).IsFlags);
            Assert.False(typeof(G<,>).IsFlags);
            Assert.False(typeof(G<int, string>).IsFlags);
            Assert.False(typeof(I1).IsFlags);
            Assert.False(typeof(IG<>).IsFlags);
            Assert.False(typeof(IG<int>).IsFlags);
        }

        [Test]
        public void IsInterfaceWorks()
        {
            Assert.False(typeof(E1).IsInterface);
            Assert.False(typeof(E2).IsInterface);
            Assert.False(typeof(C).IsInterface);
            Assert.False(typeof(G<,>).IsInterface);
            Assert.False(typeof(G<int, string>).IsInterface);
            Assert.True(typeof(I1).IsInterface);
            Assert.True(typeof(IG<>).IsInterface);
            Assert.True(typeof(IG<int>).IsInterface);
        }

        [Test]
        public void IsInstanceOfTypeWorksForReferenceTypes()
        {
            Assert.False(Type.IsInstanceOfType(new object(), typeof(IsAssignableFromTypes.C1)), "#1");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.C1(), typeof(object)), "#2");
            Assert.False(Type.IsInstanceOfType(new object(), typeof(IsAssignableFromTypes.I1)), "#3");
            Assert.False(Type.IsInstanceOfType(new IsAssignableFromTypes.C1(), typeof(IsAssignableFromTypes.D1)), "#4");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D1(), typeof(IsAssignableFromTypes.C1)), "#5");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D1(), typeof(IsAssignableFromTypes.I1)), "#6");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.C2<int>)), "#7");
            Assert.False(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.C2<string>)), "#8");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I2<int>)), "#9");
            Assert.False(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I2<string>)), "#0");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I1)), "#11");
            Assert.False(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<string>)), "#12");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<int>)), "#13");
            Assert.False(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.I2<int>)), "#14");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.I2<string>)), "#15");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I1)), "#16");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I3)), "#17");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I4)), "#18");
            Assert.True(Type.IsInstanceOfType(new IsAssignableFromTypes.X2(), typeof(IsAssignableFromTypes.I1)), "#19");
            Assert.False(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<>)), "#10");
            Assert.True(Type.IsInstanceOfType(new E2(), typeof(E1)), "#21");
            Assert.True(Type.IsInstanceOfType(new E1(), typeof(int)), "#22");
            Assert.True(Type.IsInstanceOfType(new E1(), typeof(object)), "#23");
            Assert.False(Type.IsInstanceOfType(null, typeof(object)), "#24");

            Assert.False(typeof(IsAssignableFromTypes.C1).IsInstanceOfType(new object()), "#25");
            Assert.True(typeof(object).IsInstanceOfType(new IsAssignableFromTypes.C1()), "#26");
            Assert.False(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new object()), "#27");
            Assert.False(typeof(IsAssignableFromTypes.D1).IsInstanceOfType(new IsAssignableFromTypes.C1()), "#28");
            Assert.True(typeof(IsAssignableFromTypes.C1).IsInstanceOfType(new IsAssignableFromTypes.D1()), "#29");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D1()), "#30");
            Assert.True(typeof(IsAssignableFromTypes.C2<int>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#31");
            Assert.False(typeof(IsAssignableFromTypes.C2<string>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#32");
            Assert.True(typeof(IsAssignableFromTypes.I2<int>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#33");
            Assert.False(typeof(IsAssignableFromTypes.I2<string>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#34");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#35");
            Assert.False(typeof(IsAssignableFromTypes.C2<string>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#36");
            Assert.True(typeof(IsAssignableFromTypes.C2<int>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#37");
            Assert.False(typeof(IsAssignableFromTypes.I2<int>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#38");
            Assert.True(typeof(IsAssignableFromTypes.I2<string>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#39");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D4()), "#40");
            Assert.True(typeof(IsAssignableFromTypes.I3).IsInstanceOfType(new IsAssignableFromTypes.D4()), "#41");
            Assert.True(typeof(IsAssignableFromTypes.I4).IsInstanceOfType(new IsAssignableFromTypes.D4()), "#42");
            Assert.True(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.X2()), "#43");
            Assert.False(typeof(IsAssignableFromTypes.C2<>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#44");
            Assert.True(typeof(E1).IsInstanceOfType(new E2()), "#45");
            Assert.True(typeof(int).IsInstanceOfType(new E1()), "#46");
            Assert.True(typeof(object).IsInstanceOfType(new E1()), "#47");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>()), "#48");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>()), "#49");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1X1()), "#50");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>()), "#51");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y1X1()), "#52");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X2>()), "#53");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1X2()), "#54");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X2>()), "#55");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y1X2()), "#56");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>()), "#57");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>()), "#58");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2X1()), "#59");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>()), "#60");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y2X1()), "#61");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X2>()), "#62");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2X2()), "#63");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X2>()), "#64");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y2X2()), "#65");
            Assert.False(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#66");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#67");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X1()), "#68");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>()), "#69");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X2()), "#70");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>()), "#71");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X1()), "#72");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#73");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X2()), "#74");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#75");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X1()), "#76");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>()), "#77");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X2()), "#78");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>()), "#79");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X1()), "#80");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#81");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X2()), "#82");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#83");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X1()), "#84");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>()), "#85");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X2()), "#86");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>()), "#87");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X1()), "#88");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#89");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X2()), "#90");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#91");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X1()), "#92");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>()), "#93");
            Assert.False(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X2()), "#94");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>()), "#95");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X1()), "#96");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#97");
            Assert.True(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X2()), "#98");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>()), "#99");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>()), "#100");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>()), "#101");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>()), "#102");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>()), "#103");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>()), "#104");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>()), "#105");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>()), "#106");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>()), "#107");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>()), "#108");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>()), "#109");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>()), "#110");
            Assert.True(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>()), "#111");
            Assert.False(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>()), "#112");
            Assert.False(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>()), "#113");
            Assert.True(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>()), "#114");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#115");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#116");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#117");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#118");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#119");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#120");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#121");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#122");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#123");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#124");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#125");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#126");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#127");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#128");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#129");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#130");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#131");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#132");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#133");
            Assert.True(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#134");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#135");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#136");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#137");
            Assert.False(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y5<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#138");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#139");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#140");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#141");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#142");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#143");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#144");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#145");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#146");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#147");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#148");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#149");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#150");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#151");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#152");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#153");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#154");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#155");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#156");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#157");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#158");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#159");
            Assert.False(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#160");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#161");
            Assert.True(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.I10<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>>).IsInstanceOfType(new IsAssignableFromTypes.Y6<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#162");
            Assert.False(typeof(object).IsInstanceOfType(null), "#163");
        }

        public class BaseUnnamedConstructorWithoutArgumentsTypes
        {
            public class B
            {
                public string messageB;

                public B()
                {
                    messageB = "X";
                }
            }

            public class D : B
            {
                public string messageD;

                public D()
                {
                    messageD = "Y";
                }
            }
        }

        [Test]
        public void InvokingBaseUnnamedConstructorWithoutArgumentsWorks()
        {
            var d = new BaseUnnamedConstructorWithoutArgumentsTypes.D();
            Assert.AreEqual(d.messageB + "|" + d.messageD, "X|Y");
        }

        public class BaseUnnamedConstructorWithArgumentsTypes
        {
            public class B
            {
                public string messageB;

                public B(int x, int y)
                {
                    messageB = x + " " + y;
                }
            }

            public class D : B
            {
                public string messageD;

                public D(int x, int y) : base(x + 1, y + 1)
                {
                    messageD = x + " " + y;
                }
            }
        }

        [Test]
        public void InvokingBaseUnnamedConstructorWithArgumentsWorks()
        {
            var d = new BaseUnnamedConstructorWithArgumentsTypes.D(5, 8);
            Assert.AreEqual(d.messageB + "|" + d.messageD, "6 9|5 8");
        }

        public class BaseNamedConstructorWithoutArgumentsTypes
        {
            public class B
            {
                public string messageB;

                public B()
                {
                    messageB = "X";
                }
            }

            public class D : B
            {
                public string messageD;

                public D()
                {
                    messageD = "Y";
                }
            }
        }

        [Test]
        public void InvokingBaseNamedConstructorWithoutArgumentsWorks()
        {
            var d = new BaseNamedConstructorWithoutArgumentsTypes.D();
            Assert.AreEqual(d.messageB + "|" + d.messageD, "X|Y");
        }

        public class BaseNamedConstructorWithArgumentsTypes
        {
            public class B
            {
                public string messageB;

                public B(int x, int y)
                {
                    messageB = x + " " + y;
                }
            }

            public class D : B
            {
                public string messageD;

                public D(int x, int y) : base(x + 1, y + 1)
                {
                    messageD = x + " " + y;
                }
            }
        }

        [Test]
        public void InvokingBaseNamedConstructorWithArgumentsWorks()
        {
            var d = new BaseNamedConstructorWithArgumentsTypes.D(5, 8);
            Assert.AreEqual(d.messageB + "|" + d.messageD, "6 9|5 8");
        }

        public class ConstructingInstanceWithNamedConstructorTypes
        {
            public class D
            {
                public virtual string GetMessage()
                {
                    return "The message " + f;
                }

                private string f;

                public D()
                {
                    f = "from ctor";
                }
            }

            public class E : D
            {
                public override string GetMessage()
                {
                    return base.GetMessage() + g;
                }

                private string g;

                public E()
                {
                    g = " and derived ctor";
                }
            }
        }

        [Test]
        public void ConstructingInstanceWithNamedConstructorWorks()
        {
            var d = new ConstructingInstanceWithNamedConstructorTypes.D();
            Assert.AreEqual(d.GetType(), typeof(ConstructingInstanceWithNamedConstructorTypes.D));
            Assert.True((object)d is ConstructingInstanceWithNamedConstructorTypes.D);
            Assert.AreEqual(d.GetMessage(), "The message from ctor");
        }

        [Test]
        public void ConstructingInstanceWithNamedConstructorWorks2()
        {
            var d = new ConstructingInstanceWithNamedConstructorTypes.E();
            var t = d.GetType();
            Assert.AreEqual(t, typeof(ConstructingInstanceWithNamedConstructorTypes.E), "#1");
            Assert.AreEqual(t.BaseType, typeof(ConstructingInstanceWithNamedConstructorTypes.D), "#2");
            Assert.True((object)d is ConstructingInstanceWithNamedConstructorTypes.E, "#3");
            Assert.True((object)d is ConstructingInstanceWithNamedConstructorTypes.D, "#4");
            Assert.AreEqual(d.GetMessage(), "The message from ctor and derived ctor");
        }

        public class BaseMethodInvocationTypes
        {
            public class B
            {
                public virtual int F(int x, int y)
                {
                    return x - y;
                }

                public virtual int G<T>(int x, int y)
                {
                    return x - y;
                }
            }

            public class D : B
            {
                public override int F(int x, int y)
                {
                    return x + y;
                }

                public override int G<T>(int x, int y)
                {
                    return x + y;
                }

                public int DoIt(int x, int y)
                {
                    return base.F(x, y);
                }

                public int DoItGeneric(int x, int y)
                {
                    return base.G<string>(x, y);
                }
            }
        }

        [Test]
        public void InvokingBaseMethodWorks()
        {
            Assert.AreEqual(new BaseMethodInvocationTypes.D().DoIt(5, 3), 2);
        }

        [Test]
        public void InvokingGenericBaseMethodWorks()
        {
            Assert.AreEqual(new BaseMethodInvocationTypes.D().DoItGeneric(5, 3), 2);
        }

        public class MethodGroupConversionTypes
        {
            public class C
            {
                private int m;

                public int F(int x, int y)
                {
                    return x + y + m;
                }

                public string G<T>(int x, int y)
                {
                    return x + y + m + typeof(T).Name;
                }

                public C(int m)
                {
                    this.m = m;
                }

                public Func<int, int, int> GetF()
                {
                    return F;
                }

                public Func<int, int, string> GetG()
                {
                    return G<string>;
                }
            }

            public class B
            {
                public int m;

                public virtual int F(int x, int y)
                {
                    return x + y + m;
                }

                public virtual string G<T>(int x, int y)
                {
                    return x + y + m + typeof(T).Name;
                }

                public B(int m)
                {
                    this.m = m;
                }
            }

            public class D : B
            {
                public override int F(int x, int y)
                {
                    return x - y - m;
                }

                public override string G<T>(int x, int y)
                {
                    return x - y - m + typeof(T).Name;
                }

                public Func<int, int, int> GetF()
                {
                    return base.F;
                }

                public Func<int, int, string> GetG()
                {
                    return base.G<string>;
                }

                public D(int m) : base(m)
                {
                }
            }
        }

        [Test]
        public void MethodGroupConversionWorks()
        {
            var f = new MethodGroupConversionTypes.C(4).GetF();
            Assert.AreEqual(f(5, 3), 12);
        }

        [Test]
        public void MethodGroupConversionOnGenericMethodWorks()
        {
            var f = new MethodGroupConversionTypes.C(4).GetG();
            Assert.AreEqual(f(5, 3), "12String");
        }

        [Test]
        public void MethodGroupConversionOnBaseMethodWorks()
        {
            var f = new MethodGroupConversionTypes.D(4).GetF();
            Assert.AreEqual(f(3, 5), 12);
        }

        [Test]
        public void MethodGroupConversionOnGenericBaseMethodWorks()
        {
            var g = new MethodGroupConversionTypes.C(4).GetG();
            Assert.AreEqual(g(5, 3), "12String");
        }

        [Test]
        public void ImportedInterfaceAppearsAsObjectWhenUsedAsGenericArgument()
        {
            Assert.AreEqual(typeof(BX<IImported>), typeof(BX<object>));
        }

        [Test]
        public void FalseIsFunctionShouldReturnFalse()
        {
            Assert.False((object)false is Delegate);
        }

        [Test]
        public void CastingUndefinedToOtherTypeShouldReturnUndefined()
        {
            Assert.AreEqual(Script.TypeOf((C)Script.Undefined), "undefined");
        }

        [Test]
        public void NonSerializableTypeCanInheritFromSerializableType()
        {
            var d = new DS(42);
            Assert.AreEqual(d.X, 42, "d.X");
            Assert.AreEqual(d.GetX(), 42, "d.GetX");
        }

        [Test]
        public void InheritingFromRecordWorks()
        {
            var c = new CS2() { X = 42 };
            Assert.AreEqual(c.X, 42);
        }

        [Test]
        public void InstanceOfWorksForSerializableTypesWithCustomTypeCheckCode()
        {
            object o1 = new { x = 1 };
            object o2 = new { x = 1, y = 2 };
            Assert.False(typeof(DS2).IsInstanceOfType(o1), "o1 should not be of type");
            //Assert.True (typeof(DS2).IsInstanceOfType(o2), "o2 should be of type");
        }

        [Test]
        public void StaticGetTypeMethodWorks()
        {
            Assert.AreEqual(Type.GetType("Bridge.ClientTest.Reflection.TypeSystemTests"), typeof(TypeSystemTests), "#1");
            Assert.AreEqual(Type.GetType("Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest"), typeof(TypeSystemTests), "#2");
            Assert.AreEqual(Type.GetType("Bridge.ClientTest.Reflection.TypeSystemTests, mscorlib"), null, "#3");
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2, mscorlib"), typeof(Dictionary<,>), "#4");
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2, NotLoaded.Assembly"), null, "#5");
        }

        [Test]
        public void StaticGetTypeMethodWithGenericsWorks()
        {
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2[[String],[Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest]]"), typeof(Dictionary<string, TypeSystemTests>), "#1");
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2[[Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest],[String]]"), typeof(Dictionary<TypeSystemTests, string>), "#2");
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2[[System.Int32, mscorlib],[Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest]]"), typeof(Dictionary<int, TypeSystemTests>), "#3");
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2[[String],[Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest]], mscorlib"), typeof(Dictionary<string, TypeSystemTests>), "#4");
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2[[Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest],[String]], mscorlib"), typeof(Dictionary<TypeSystemTests, string>), "#5");
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2[[Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest],[Bridge.ClientTest.Reflection.TypeSystemTests, Bridge.ClientTest]], mscorlib"), typeof(Dictionary<TypeSystemTests, TypeSystemTests>), "#6");
            Assert.AreEqual(Type.GetType("System.Collections.Generic.Dictionary$2[[String],[System.Collections.Generic.Dictionary$2[[System.Collections.Generic.Dictionary$2[[System.Int32, mscorlib],[Date]], mscorlib],[System.Collections.Generic.Dictionary$2[[System.Int32, mscorlib],[System.Double]], mscorlib]], mscorlib]], mscorlib"), typeof(Dictionary<string, Dictionary<Dictionary<int, DateTime>, Dictionary<int, double>>>), "#7");
        }

        [Enum(Emit.StringName)]
        public enum NamedValuesEnum
        {
            FirstValue,
            SecondValue,
        }

        [Enum(Emit.StringName)]
        public enum ImportedNamedValuesEnum
        {
            FirstValue,
            SecondValue,
        }

        private bool DoesItThrow(Action a)
        {
            try
            {
                a();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool IsOfType<T>(object o)
        {
            return o is T;
        }

        private T GetDefault<T>()
        {
            return default(T);
        }

        [Test]
        public void CastingToNamedValuesEnumCastsToString()
        {
            Assert.True((object)NamedValuesEnum.FirstValue is NamedValuesEnum, "#1");
            Assert.True((object)"firstValue" is NamedValuesEnum, "#2");
            Assert.False((object)(int)0 is NamedValuesEnum, "#3");
#pragma warning disable 219
            Assert.False(DoesItThrow(() => { var x = (NamedValuesEnum)(object)"firstValue"; }), "#4");
            Assert.True(DoesItThrow(() => { var x = (NamedValuesEnum)(object)0; }), "#5");
#pragma warning restore 219

            Assert.NotNull((object)NamedValuesEnum.FirstValue as NamedValuesEnum?, "#6");
            Assert.NotNull((object)"firstValue" as NamedValuesEnum?, "#7");
            Assert.Null((object)(int)0 as NamedValuesEnum?, "#8");

            Assert.True(IsOfType<NamedValuesEnum>((object)NamedValuesEnum.FirstValue), "#9");
            Assert.True(IsOfType<NamedValuesEnum>("firstValue"), "#10");
            Assert.False(IsOfType<NamedValuesEnum>(0), "#11");
        }

        [Test]
        public void CastingToImportedNamedValuesEnumCastsToString()
        {
            Assert.True((object)ImportedNamedValuesEnum.FirstValue is ImportedNamedValuesEnum, "#1");
            Assert.True((object)"firstValue" is ImportedNamedValuesEnum, "#2");
            Assert.False((object)(int)0 is ImportedNamedValuesEnum, "#3");
#pragma warning disable 219
            Assert.False(DoesItThrow(() => { var x = (ImportedNamedValuesEnum)(object)"firstValue"; }), "#4");
            Assert.True(DoesItThrow(() => { var x = (ImportedNamedValuesEnum)(object)0; }), "#5");
#pragma warning restore 219

            Assert.NotNull((object)ImportedNamedValuesEnum.FirstValue as ImportedNamedValuesEnum?, "#6");
            Assert.NotNull((object)"firstValue" as ImportedNamedValuesEnum?, "#7");
            Assert.Null((object)(int)0 as ImportedNamedValuesEnum?, "#8");
        }

        [Test]
        public void DefaultValueOfNamedValuesEnumIsNull()
        {
            Assert.Null(default(NamedValuesEnum), "#1");
            Assert.Null(GetDefault<NamedValuesEnum>(), "#2");
        }

        [Test]
        public void DefaultValueOfImportedNamedValuesEnumIsNull()
        {
            Assert.Null(default(ImportedNamedValuesEnum), "#1");
            Assert.Null(GetDefault<ImportedNamedValuesEnum>(), "#2");
        }
    }
}