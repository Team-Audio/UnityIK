using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class floatExtensions
{
    public static Vector3 ToVec3(this float value)
    {
        return new Vector3(value, value, value);

    }
}
