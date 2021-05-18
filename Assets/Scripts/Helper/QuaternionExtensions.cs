using UnityEngine;

namespace Helper
{
    public static class QuaternionExtensions
    {
        public static Quaternion Inverse(this Quaternion self)
        {
            return Quaternion.Inverse(self);
        }
    }
}