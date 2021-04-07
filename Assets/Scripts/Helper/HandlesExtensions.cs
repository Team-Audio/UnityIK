using UnityEditor;
using UnityEngine;

public static class CustomHandles
{
    public static void DrawSolidDisc(Vector3 position, Vector3 normal, float radius, Color color=default)
    {
        if (color == default) color = Color.black;
        Color tmp = Handles.color;
        Handles.color = color;
        Handles.DrawSolidDisc(position, normal, radius);
        Handles.color = tmp;
    }
    public static void DrawLine(Vector3 position, Vector3 endPos, Color color=default,float thickness=1.0f)
    {
        if (color == default) color = Color.black;
        Color tmp = Handles.color;
        Handles.color = color;
        Handles.DrawLine(position, endPos, thickness);
        Handles.color = tmp;
    }
}
