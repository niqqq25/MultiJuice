using System;
using UnityEngine;

public class ByteBuffer : INetBuffer
{
    public CompressionSchema CompressionSchema { get; set; }
    public byte[] Data;
    public int Head;

    public ByteBuffer(int length)
    {
        Data = new byte[length];
        Head = 0;
    }

    public ByteBuffer(byte[] bytes)
    {
        Data = bytes;
        Head = 0;
    }

    private bool ExceedsBuffer(int size)
    {
        if (size <= 0 || Head + size > Data.Length)
        {
            Console.WriteLine("The maximum buffer size has been exceeded");
            return true;
        }
        return false;
    }

    public byte[] GetBytes()
    {
        byte[] bytes = new byte[Head];
        Buffer.BlockCopy(Data, 0, bytes, 0, Head);
        return bytes;
    }

    public byte[] ReadBytes(int size)
    {
        if (ExceedsBuffer(size)) return new byte[0];

        byte[] bytes = new byte[size];
        Buffer.BlockCopy(Data, Head, bytes, 0, size);
        Head += size;
        return bytes;
    }

    public byte GetByte(bool delta = false) => ReadBytes(1)[0];
    public int GetInt(bool delta = false) => BitConverter.ToInt32(ReadBytes(4), 0);
    public uint GetUInt(bool delta = false) => BitConverter.ToUInt32(ReadBytes(4), 0);

    public float GetFloat(bool delta = false)
    {
        if (delta)
        {
            bool changed = GetBool();
            if (!changed) return 0f;
        }

        return BitConverter.ToSingle(ReadBytes(4), 0);
    }

    public bool GetBool(bool delta = false) => BitConverter.ToBoolean(ReadBytes(1), 0);

    public short GetShort(bool delta = false)
    {
        if (delta)
        {
            bool changed = GetBool();
            if (!changed) return 0;
        }

        return BitConverter.ToInt16(ReadBytes(2), 0);
    }

    public void WriteBytes(byte[] bytes)
    {
        if (ExceedsBuffer(bytes.Length)) return;

        Buffer.BlockCopy(bytes, 0, Data, Head, bytes.Length);
        Head += bytes.Length;
    }

    public void WriteBytes(byte[] bytes, int size)
    {
        if (ExceedsBuffer(size)) return;

        Buffer.BlockCopy(bytes, 0, Data, Head, bytes.Length < size ? bytes.Length : size);
        Head += size;
    }

    public void Put(byte value, bool delta = false)
    {
        if (ExceedsBuffer(1)) return;
        Data[Head] = value;
        Head++;
    }
    public void Put(int value, bool delta = false) => WriteBytes(BitConverter.GetBytes(value));
    public void Put(uint value, bool delta = false) => WriteBytes(BitConverter.GetBytes(value));

    public void Put(float value, bool delta = false)
    {
        if (delta)
        {
            if (Mathf.Approximately(value, 0))
            {
                Put(false);
                return;
            }
            Put(true);
        }

        WriteBytes(BitConverter.GetBytes(value));
    }

    public void Put(bool value, bool delta = false) => WriteBytes(BitConverter.GetBytes(value));

    public void Put(short value, bool delta = false)
    {
        if (delta)
        {
            if (value == 0)
            {
                Put(false);
                return;
            }
            Put(true);
        }

        WriteBytes(BitConverter.GetBytes(value));
    }

    public void Reset()
    {
        Head = 0;
    }

    public void Load(byte[] data)
    {
        Data = data;
        Head = 0;
    }

    public void SkipBytes(int count)
    {
        Head += count;
    }
}
