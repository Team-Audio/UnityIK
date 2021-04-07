using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
namespace Extensions
{
    public static class MathExtensions
    {
        public static float3 ProjectOnPlane(float3 vec, float3 pointO, float3 planeNormal)
        {
            return vec - (math.dot(planeNormal, vec) - math.dot(planeNormal, pointO)) * planeNormal;
        }
        public static float3 RotateAboutAxisDeg(float3 sourceVec, float angleDeg, float3 rotationAxis)
        {
            Quaternion rotation = Quaternion.AngleAxis(angleDeg, rotationAxis);
            return rotation * sourceVec;
        }
        public static float singedAngleDeg(float3 refVec, float3 otherVec, float3 normal)
        {
            //use dot product to get angle, convert to degrees
            float dot = math.dot(refVec, otherVec);
            float unsignedAngle = math.acos(dot);
            unsignedAngle = unsignedAngle * 180 / math.PI;
            float sign = math.sign(math.dot(math.cross(refVec, otherVec), normal));
            return unsignedAngle * sign;
        }
    }
}
