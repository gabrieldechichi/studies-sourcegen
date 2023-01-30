using System.Diagnostics;

namespace ConsoleApp
{
    public class PolymorphicStruct : Attribute
    {
        
    }
    
    [PolymorphicStruct]
    public interface IMyTestInterface
    {
        public void Foo();
    }
    
    public partial struct MyStructA : IMyTestInterface
    {
        public int Field1;
        public int Field2;
        public void Foo()
        {
            Console.WriteLine("FOO from MyStructA");
        }
    }
    public partial struct MyStructB : IMyTestInterface
    {
        public int Field;
        public bool OtherField;
        public void Foo()
        {
            Console.WriteLine("FOO from MyStructB");
        }
    }
    
    public struct StructWithNoInterface{}
    
    partial class Program
    {
        static void Main(string[] args)
        {
            var t = new MyTestInterface();
        }

        // static partial void HelloFrom(string name);
    }
}