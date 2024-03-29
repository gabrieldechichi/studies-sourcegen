using NUnit.Framework.Interfaces;

namespace PolymorphicStructsTests
{
    public class PolymorphicStructShould
    {
        //work with multiple interfaces
        //work with interface hierarchies
        //TODO: Work with properties

        [Test]
        public void MergeCommonFields()
        {
            int GetFieldCountOfType<T>(Type t)
            {
                return t.GetFields().Count(f => f.FieldType == typeof(T));
            }

            var intFieldCountA = GetFieldCountOfType<int>(typeof(FieldStructA));
            var intFieldCountB = GetFieldCountOfType<int>(typeof(FieldStructB));
            var intFieldCountC = GetFieldCountOfType<int>(typeof(FieldStructC));

            Assert.That(intFieldCountA > 0 && intFieldCountB > 0 && intFieldCountC > 0, Is.True);

            var max = Math.Max(Math.Max(intFieldCountA, intFieldCountB), intFieldCountC);
            var mergedFieldCount = GetFieldCountOfType<int>(typeof(FieldInterface));
            Assert.That(mergedFieldCount, Is.EqualTo(max));
        }

        [Test]
        public void KeepDataWhenInBaseClassForm()
        {
            var a = new FieldStructA() { Int1 = 1, Int2 = 2 }.ToFieldInterface();
            var b = new FieldStructB() { Int = 3 }.ToFieldInterface();

            Assert.That(a.Int32_0, Is.EqualTo(1));
            Assert.That(a.Int32_1, Is.EqualTo(2));
            Assert.That(b.Int32_0, Is.EqualTo(3));
            Assert.That(b.Int32_1, Is.EqualTo(0));
        }

        [Test]
        public void ChooseCorrectImplementation_WithReturnValue()
        {
            var a = new CorrectImplementationA
            {
                A = 1
            }.ToCorrectImplementationInterface();
            var b = new CorrectImplementationB
            {
                A = 1,
                B = 2
            }.ToCorrectImplementationInterface();

            Assert.That(a.Foo(), Is.EqualTo(1));
            Assert.That(b.Foo(), Is.EqualTo(3));
        }

        [Test]
        public void ChooseCorrectImplementation_WithVoidReturn()
        {
            var a = new VoidMethodA()
            {
                A = 1
            }.ToVoidMethodInterface();
            var b = new VoidMethodB()
            {
                B = 2
            }.ToVoidMethodInterface();

            a.Foo();
            b.Foo();

            Assert.That(new VoidMethodA(a).A, Is.EqualTo(2));
            Assert.That(new VoidMethodB(b).B, Is.EqualTo(1));
        }

        [Test]
        public void ThrowExceptionIfUnexpectedTypeId()
        {
            var a = new CorrectImplementationA().ToCorrectImplementationInterface();

            //completely wrong typeid
            a.CurrentTypeId = (CorrectImplementationInterface.TypeId)1000;
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Foo());
        }


        [Test]
        public void WorkWithRefOutInParameters()
        {
            var a = new ParameterMethodA().ToParameterMethodInterface();
            var b = new ParameterMethodB().ToParameterMethodInterface();

            switch (a.CurrentTypeId)
            {
                case ParameterMethodInterface.TypeId.ParameterMethodA:
                    break;
                case ParameterMethodInterface.TypeId.ParameterMethodB:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            var n = 1;
            a.Foo(ref n, false, out var c, "");

            Assert.That(n, Is.EqualTo(100));
            Assert.That(c, Is.EqualTo(1));

            var boolean = true;
            var someString = "Other string";
            b.Foo(ref n, boolean, out c, someString);

            Assert.That(n, Is.EqualTo(-100));
            Assert.That(c, Is.EqualTo(someString.Length));
        }

        [Test]
        public void SupportMultipleImplementations()
        {
            int CallPerformAction<T>(T action) where T : struct, IAction
            {
                return action.PerformAction();
            }

            var a = new MeleeActionImpl().ToMeleeAction();
            var b = new RangedActionImpl().ToRangedAction();
            
            Assert.That(CallPerformAction(a), Is.EqualTo(1));
            Assert.That(CallPerformAction(b), Is.EqualTo(2));
        }
    }

    #region MergeFieldTest

    [PolymorphicStruct]
    public interface IFieldInterface
    {
    }

    public partial struct FieldStructA : IFieldInterface
    {
        public int Int1;
        public int Int2;
    }

    public partial struct FieldStructB : IFieldInterface
    {
        public int Int;
    }

    public partial struct FieldStructC : IFieldInterface
    {
        public bool Bool;
        public int Int;
    }

    #endregion

    #region CorrectImplementationTest

    [PolymorphicStruct]
    public interface ICorrectImplementationInterface
    {
        public int Foo();
    }

    public partial struct CorrectImplementationA : ICorrectImplementationInterface
    {
        public int A;

        public int Foo()
        {
            return A;
        }
    }

    public partial struct CorrectImplementationB : ICorrectImplementationInterface
    {
        public int A;
        public int B;

        public int Foo()
        {
            return A + B;
        }
    }

    #endregion

    #region VoidMethodTest

    [PolymorphicStruct]
    public interface IVoidMethodInterface
    {
        public void Foo();
    }

    public partial struct VoidMethodA : IVoidMethodInterface
    {
        public int A;

        public void Foo()
        {
            A++;
        }
    }

    public partial struct VoidMethodB : IVoidMethodInterface
    {
        public int B;

        public void Foo()
        {
            B--;
        }
    }

    #endregion

    #region RefOutInParametersTest

    [PolymorphicStruct]
    public interface IParameterMethodInterface
    {
        public void Foo(ref int a, in bool b, out int c, string s);
    }

    public partial struct ParameterMethodA : IParameterMethodInterface
    {
        public void Foo(ref int a, in bool b, out int c, string s)
        {
            c = 1;
            a = 100;
        }
    }

    public partial struct ParameterMethodB : IParameterMethodInterface
    {
        public void Foo(ref int a, in bool b, out int c, string s)
        {
            c = s.Length;
            a = -100;
        }
    }

    #endregion

    #region MultipleImplementationsTest


    public interface IAction
    {
        int PerformAction();
    }

    [PolymorphicStruct]
    public interface IMeleeAction : IAction
    {
    }

    [PolymorphicStruct]
    public interface IRangedAction : IAction
    {
    }

    public partial struct MeleeActionImpl : IMeleeAction
    {
        public int PerformAction()
        {
            return 1;
        }
    }

    public partial struct RangedActionImpl : IRangedAction
    {
        public int PerformAction()
        {
            return 2;
        }
    }
    #endregion
}