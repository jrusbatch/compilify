using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Compilify.Utilities
{
    public static class Base36Encoder
    {
        private const string Characters = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string Encode(int i)
        {
            if (i == 0)
            {
                return Characters[0].ToString(CultureInfo.InvariantCulture);
            }

            var @base = Characters.Length;
            var slug = new Stack<char>(7);

            while (i > 0)
            {
                slug.Push(Characters[i % @base]);
                i /= @base;
            }

            return new string(slug.ToArray());
        }

        public static int? Decode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            var @base = Characters.Length;
            return str.ToLowerInvariant().Aggregate(0, (current, c) => (current * @base) + Characters.IndexOf(c));
        }
    }
}
