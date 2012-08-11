namespace Compilify.Utilities
{
    internal static class Hash
    {
        internal static int Combine(int newKey, int currentKey)
        {
            return (currentKey * -1521134295) + newKey;
        }

        internal static int Combine<T>(T newKeyPart, int currentKey)
        {
            var num = currentKey * -1521134295;

            if (!ReferenceEquals(newKeyPart, null))
            {
                return num + newKeyPart.GetHashCode();
            }

            return num;
        }
    }
}
