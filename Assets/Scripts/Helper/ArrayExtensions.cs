using System;
using System.Net.Mail;

namespace Helper
{
    public static class ArrayExtensions
    {
        public static void Generate<T>(this T[] self, Func<T> predicate)
        {
            for (int i = 0; i < self.Length; ++i)
            {
                self[i] = predicate.Invoke();
            }
        }

        public static int End<T>(this T[] self)
        {
            return self.Length - 1;
        }

        public static T First<T>(this T[] self)
        {
            return self[0];
        }

        public static T Last<T>(this T[] self)
        {
            return self[self.Length - 1];
        }
        
    }
}