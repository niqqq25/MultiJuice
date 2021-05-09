using System;
using UnityEngine;

public static  class SmallestThree
{
    const float QUAT_FLOAT_PRECISION_MULT = 32767f;

    public static void EncodeQuaternion(INetBuffer writer, Quaternion quaternion, bool delta = false)
    {
        byte maxIndex = 0;
        float maxValue = float.MinValue;
        float maxValueSign = 1;

        // Find the largest value in the quaternion and save its index.
        for (byte i = 0; i < 4; i++)
        {
            var value = quaternion[i];
            var absValue = Mathf.Abs(value);
            if (absValue > maxValue)
            {
                maxIndex = i;
                maxValue = absValue;

                // Note the sign of the maxValue for later.
                maxValueSign = Mathf.Sign(value);
            }
        }

        // Encode the smallest three components.
        short a, b, c;
        switch (maxIndex)
        {
            case 0:
                a = (short)(quaternion.y * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                b = (short)(quaternion.z * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                c = (short)(quaternion.w * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                break;
            case 1:
                a = (short)(quaternion.x * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                b = (short)(quaternion.z * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                c = (short)(quaternion.w * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                break;
            case 2:
                a = (short)(quaternion.x * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                b = (short)(quaternion.y * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                c = (short)(quaternion.w * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                break;
            case 3:
                a = (short)(quaternion.x * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                b = (short)(quaternion.y * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                c = (short)(quaternion.z * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                break;
            default:
                throw new InvalidProgramException("Unexpected quaternion index.");
        }

        bool deltaZero = GameConfig.deltaZeroDeep && delta;

        writer.Put(maxIndex);
        writer.Put(a, deltaZero);
        writer.Put(b, deltaZero);
        writer.Put(c, deltaZero);
    }

    public static Quaternion DecodeQuaternion(INetBuffer reader, bool delta = false)
    {
        bool deltaZero = reader.CompressionSchema.DeltaZeroDeep && delta;

        byte maxIndex = reader.GetByte();
        float a = reader.GetShort(deltaZero) / QUAT_FLOAT_PRECISION_MULT;
        float b = reader.GetShort(deltaZero) / QUAT_FLOAT_PRECISION_MULT;
        float c = reader.GetShort(deltaZero) / QUAT_FLOAT_PRECISION_MULT;

        float d = Mathf.Sqrt(1f - (a * a + b * b + c * c));
        switch (maxIndex)
        {
            case 0:
                return new Quaternion(d, a, b, c);
            case 1:
                return new Quaternion(a, d, b, c);
            case 2:
                return new Quaternion(a, b, d, c);
            case 3:
                return new Quaternion(a, b, c, d);
            default:
                throw new InvalidProgramException("Unexpected quaternion index.");
        }
    }
}
