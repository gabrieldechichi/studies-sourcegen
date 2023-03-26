
namespace Game
{
    public static class EnumExtensions
    {
        public static string ToStringFast(this Colour e)
        {
            return e switch
            {
                Colour.Red => nameof(Colour.Red),
                Colour.Blue => nameof(Colour.Blue),
                _ => throw new Exception()
            };
        }
    }
}