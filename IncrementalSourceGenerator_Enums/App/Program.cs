using Game;

Console.WriteLine($"{Colour.Red}");
Console.WriteLine(Colour.Red.ToStringFast());

public class EnumExtensions : Attribute
{
}

[EnumExtensions]
public enum Colour // Yes, I'm British
{
    Red = 0,
    Blue = 1,
};