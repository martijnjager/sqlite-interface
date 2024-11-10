using Humanizer;

namespace Database.Extensions
{
    public static class Strings
    {
        public static string Singular(this string str)
        {
            return str.Singularize(false);
        }

        public static string Plural(this string str)
        {
            return str.Pluralize(false);
        }
    }
}
