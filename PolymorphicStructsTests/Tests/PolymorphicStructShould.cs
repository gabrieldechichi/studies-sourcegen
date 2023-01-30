namespace PolymorphicStructsTests
{
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
    
    public class PolymorphicStructShould
    {
        //merge common fields
        //choose correct implementation
        //work for functions with no return value
        //keep field changes
        //work with ref, out, in
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
    }
}