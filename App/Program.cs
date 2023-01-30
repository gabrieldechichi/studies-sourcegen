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
    
    public struct MyStruct : IMyTestInterface
    {
        public void Foo()
        {
            Console.WriteLine("FOO from MyStruct");
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
