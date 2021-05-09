using System;
using UnityEngine;

public static class Utils
{
    public static Vector3 EulerAnglesBetween(Quaternion from, Quaternion to)
    {
        Vector3 delta = to.eulerAngles - from.eulerAngles;

        if (delta.x > 180)
            delta.x -= 360;
        else if (delta.x < -180)
            delta.x += 360;

        if (delta.y > 180)
            delta.y -= 360;
        else if (delta.y < -180)
            delta.y += 360;

        if (delta.z > 180)
            delta.z -= 360;
        else if (delta.z < -180)
            delta.z += 360;

        return delta;
    }

    public static Vector3 QuaternionToDirection(Quaternion quaternion)
    {
        Vector3 eulerAngles = quaternion.eulerAngles;
        float angle = Mathf.Deg2Rad * eulerAngles.z;

        return new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0);
    }

    public static unsafe uint SingleToUInt32Bits(float value)
    {
        return *((uint*)&value);
    }

    public static unsafe float UInt32BitsToSingle(uint value)
    {
        return *((float*)&value);
    }

    public static void ChangeObjectOpacity(Transform transform, float opacity)
    {
        SpriteRenderer spriteRenderer;
        Color color;

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            spriteRenderer = transform.GetChild(i).GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                color = spriteRenderer.color;
                color.a = opacity;
                spriteRenderer.color = color;
            }
        }
    }
}
