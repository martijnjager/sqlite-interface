using Humanizer;
using System.Text;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// Removes special characters from a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters(this string input)
        {
            return Regex.Replace(input, @"[^a-zA-Z]", string.Empty);
        }

        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToLower(input[0]));

            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    stringBuilder.Append('_');
                    stringBuilder.Append(char.ToLower(input[i]));
                }
                else
                {
                    stringBuilder.Append(input[i]);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
