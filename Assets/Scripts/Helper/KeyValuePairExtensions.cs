using System.Collections.Generic;

namespace Helper
{
    public static class KeyValuePairExtensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey v1, out TValue v2)
        {
            v1 = self.Key;
            v2 = self.Value;
        }
    }
}