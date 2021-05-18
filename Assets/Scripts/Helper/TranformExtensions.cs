using System;
using UnityEngine;

namespace Helper
{
    public static class TranformExtensions
    {
        public static Transform FindNthParent(this Transform self,int N)
        {
            var current = self;
            
            for (var i = 0; i <= N; i++)
            {
                if (!current)
                    throw new ArgumentException("The chain value is longer than the ancestor chain!");
                current = current.parent;
            }

            return current;

        }
    }
}