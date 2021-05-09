using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using LiteNetLib.Utils;
using UnityEngine;

public static class MyExtensions
{
    public static Vector3 GetVector3(this NetDataReader dataReader)
    {
        float x = dataReader.GetFloat();
        float y = dataReader.GetFloat();
        float z = dataReader.GetFloat();

        return new Vector3(x, y, z);
    }

    public static void Put(this NetDataWriter dataWriter, Quaternion value)
    {
        dataWriter.Put(value.x);
        dataWriter.Put(value.y);
        dataWriter.Put(value.z);
        dataWriter.Put(value.w);
    }

    public static Quaternion GetQuaternion(this NetDataReader dataReader)
    {
        float x = dataReader.GetFloat();
        float y = dataReader.GetFloat();
        float z = dataReader.GetFloat();
        float w = dataReader.GetFloat();

        return new Quaternion(x, y, z, w);
    }

    public static void Put(this INetBuffer writer, Vector3 value, bool delta = false)
    {
        if (GameConfig.deltaZeroCompression && delta)
        {
            if (Vector3.SqrMagnitude(value) < 0.0001)
            {
                writer.Put(false);
                return;
            }
            writer.Put(true);
        }

        if (GameConfig.deltaZeroDeep && delta)
        {
            writer.Put(value.x, true);
            writer.Put(value.y, true);
            writer.Put(value.z, true);
        }
        else
        {
            writer.Put(value.x);
            writer.Put(value.y);
            writer.Put(value.z);
        }
    }

    public static Vector3 GetVector3(this INetBuffer reader, bool delta = false)
    {
        if (reader.CompressionSchema.DeltaZeroCompression && delta)
        {
            bool changed = reader.GetBool();
            if (!changed) return Vector3.zero;
        }

        if(reader.CompressionSchema.DeltaZeroDeep && delta)
        {
            return new Vector3(reader.GetFloat(true), reader.GetFloat(true), reader.GetFloat(true));
        }

        return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }

    public static void Put(this INetBuffer writer, Quaternion value, bool delta = false)
    {
        if (GameConfig.deltaZeroCompression && delta)
        {
            if (value == Quaternion.identity)
            {
                writer.Put(false);
                return;
            }
            writer.Put(true);
        }

        bool deltaZero = GameConfig.deltaZeroDeep && delta;

        if (GameConfig.smallestThree)
        {
            SmallestThree.EncodeQuaternion(writer, value, delta);
        }
        else
        {
            writer.Put(value.x, deltaZero);
            writer.Put(value.y, deltaZero);
            writer.Put(value.z, deltaZero);
            writer.Put(value.w, deltaZero);
        }
    }

    public static Quaternion GetQuaternion(this INetBuffer reader, bool delta = false)
    {
        if (reader.CompressionSchema.DeltaZeroCompression && delta)
        {
            bool changed = reader.GetBool();
            if (!changed) return Quaternion.identity;
        }

        bool deltaZero = reader.CompressionSchema.DeltaZeroDeep && delta;

        if (reader.CompressionSchema.SmallestThree)
        {
            return SmallestThree.DecodeQuaternion(reader, delta);
        }

        float x = reader.GetFloat(deltaZero);
        float y = reader.GetFloat(deltaZero);
        float z = reader.GetFloat(deltaZero);
        float w = reader.GetFloat(deltaZero);

        return new Quaternion(x, y, z, w);
    }

    public static void DequeueChunk<T>(this Queue<T> queue, int chunkSize)
    {
        for (int i = 0; i < chunkSize && queue.Count > 0; i++)
        {
            queue.Dequeue();
        }
    }

    public static bool IsEqual(this Vector3 first, Vector3 second, float allowedDifference)
    {
        var dx = first.x - second.x;
        if (Mathf.Abs(dx) > allowedDifference)
            return false;

        var dy = first.y - second.y;
        if (Mathf.Abs(dy) > allowedDifference)
            return false;

        var dz = first.z - second.z;

        return Mathf.Abs(dz) <= allowedDifference;
    }

    public static void RemoveLowerThanAnd<T>(this Dictionary<int, T> dictionary, int index)
    {
        HashSet<int> keysToRemove = new HashSet<int>();

        foreach (var key in dictionary.Keys)
        {
            if (key <= index)
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
            dictionary.Remove(key);
    }

    public static void RemoveLowerThan<T>(this Dictionary<int, T> dictionary, int index)
    {
        HashSet<int> keysToRemove = new HashSet<int>();

        foreach (var key in dictionary.Keys)
        {
            if (key < index)
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
            dictionary.Remove(key);
    }

    public static Bounds OrthographicBounds(this Camera camera)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        Bounds bounds = new Bounds(
            camera.transform.position,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
        return bounds;
    }

    public static bool TryParse(this IPEndPoint _, string endPoint, out IPEndPoint iPEndPoint)
    {
        string[] ep = endPoint.Split(':');
        iPEndPoint = null;

        if (ep.Length != 2) return false;
        IPAddress ip;
        if (!IPAddress.TryParse(ep[0], out ip))
        {
            return false;
        }
        int port;
        if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
        {
            return false;
        }

        iPEndPoint = new IPEndPoint(ip, port);
        return true;
    }
}
