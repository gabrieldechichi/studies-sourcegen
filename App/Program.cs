using System.Diagnostics;

namespace ConsoleApp
{
    public class GenerateMethodAtribute : Attribute
    {
        
    }

    [GenerateMethodAtribute]
    public partial class MyClass
    {
        
    }
    
    partial class Program
    {
        static void Main(string[] args)
        {
            HelloFrom("Generated Code");

            var c = new MyClass();
            c.MyGeneratedMethod();
            Console.WriteLine("Test");
        }

        // static partial void HelloFrom(string name);
    }
}
