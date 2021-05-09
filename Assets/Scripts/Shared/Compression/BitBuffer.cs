using System;
using UnityEngine;

public class BitBuffer : INetBuffer
{
    public CompressionSchema CompressionSchema { get; set; }
    int head;
    private uint[] chunks;

    public BitBuffer(int byteLength = 100)
    {
        chunks = new uint[byteLength / 4];
        head = 0;
    }

    public void Write(int numBits, uint value)
    {
        int index = head >> 5;
        int used = head & 0x0000001F;

        if ((index + 1) >= chunks.Length)
        {
            throw new Exception("Exceeds limit");
        }

        ulong chunkMask = ((1UL << used) - 1);
        ulong scratch = chunks[index] & chunkMask;
        ulong result = scratch | ((ulong)value << used);

        chunks[index] = (uint)result;
        chunks[index + 1] = (uint)(result >> 32);

        head += numBits;
    }

    public uint Read(int numBits)
    {
        int index = head >> 5;
        int used = head & 0x0000001F;

        ulong chunkMask = ((1UL << numBits) - 1) << used;
        ulong scratch = chunks[index];
        if ((index + 1) < chunks.Length)
            scratch |= (ulong)chunks[index + 1] << 32;
        ulong result = (scratch & chunkMask) >> used;

        head += numBits;
        return (uint)result;
    }

    public byte[] GetBytes()
    {
        int numChunks = (head >> 5) + 1;
        byte[] data = new byte[numChunks * 4];

        for (int i = 0; i < numChunks; i++)
        {
            int dataIdx = i * 4;
            uint chunk = chunks[i];
            data[dataIdx] = (byte)(chunk);
            data[dataIdx + 1] = (byte)(chunk >> 8);
            data[dataIdx + 2] = (byte)(chunk >> 16);
            data[dataIdx + 3] = (byte)(chunk >> 24);
        }

        return data;
    }

    public void Load(byte[] data)
    {
        int numChunks = data.Length / 4;
        if (chunks.Length < numChunks)
        {
            chunks = new uint[numChunks];
        }
        else
        {
            Array.Clear(chunks, 0, chunks.Length);
        }

        for (int i = 0; i < numChunks; i++)
        {
            int dataIdx = i * 4;
            uint chunk =
              (uint)data[dataIdx] |
              (uint)data[dataIdx + 1] << 8 |
              (uint)data[dataIdx + 2] << 16 |
              (uint)data[dataIdx + 3] << 24;
            chunks[i] = chunk;
        }

        head = 0;
    }

    public void Put(byte val, bool delta = false)
    {
        Write(8, val);
    }

    public void Put(uint val, bool delta = false)
    {
        if (delta)
        {
            if (val == 0)
            {
                Put(false);
                return;
            }
            Put(true);
        }

        uint buffer;

        do
        {
            buffer = val & 0x7Fu;
            val >>= 7;

            if (val > 0)
                buffer |= 0x80u;

            Write(8, buffer);
        }
        while (val > 0);
    }

    public void Put(int val, bool delta = false)
    {
        uint zigzag = (uint)((val << 1) ^ (val >> 31));
        Put(zigzag, delta);
    }

    public void Put(bool value, bool delta = false)
    {
        Write(1, value ? 1U : 0U);
    }

    public void Put(float val, bool delta = false)
    {
        if (delta)
        {
            if(Mathf.Approximately(val, 0))
            {
                Put(false);
                return;
            }
            Put(true);
        }

        Write(32, Utils.SingleToUInt32Bits(val));
    }

    public void Put(short val, bool delta = false)
    {
        Put((int)val, delta);
    }

    public int GetInt(bool delta = false)
    {
        uint val = GetUInt(delta);
        int zagzig = (int)((val >> 1) ^ (-(val & 1)));
        return zagzig;
    }

    public uint GetUInt(bool delta = false)
    {
        uint buffer;
        uint val = 0x0u;
        int s = 0;

        if(delta)
        {
            bool changed = GetBool();
            if (!changed) return 0;
        }

        do
        {
            buffer = Read(8);

            val |= (buffer & 0x7Fu) << s;
            s += 7;

            // Continue if we're flagged for more
        } while ((buffer & 0x80u) > 0);

        return val;
    }

    public byte GetByte(bool delta = false)
    {
        return (byte)Read(8);
    }

    public bool GetBool(bool delta = false)
    {
        return Read(1) > 0;
    }

    public float GetFloat(bool delta = false)
    {
        if(delta)
        {
            bool changed = GetBool();
            if (!changed) return 0f;
        }

        return Utils.UInt32BitsToSingle(Read(32));
    }

    public short GetShort(bool delta = false)
    {
        return (short)GetInt(delta);
    }

    public void Reset()
    {
        head = 0;
    }

    public void SkipBytes(int count)
    {
        head += count * 8;
    }
}