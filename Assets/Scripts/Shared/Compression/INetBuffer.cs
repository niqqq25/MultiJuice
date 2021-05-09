using System;

public interface INetBuffer
{
    public CompressionSchema CompressionSchema { get; set; }

    public void Put(byte value, bool delta = false);
    public void Put(int value, bool delta = false);
    public void Put(uint value, bool delta = false);
    public void Put(bool value, bool delta = false);
    public void Put(float value, bool delta = false);
    public void Put(short value, bool delta = false);

    public byte GetByte(bool delta = false);
    public int GetInt(bool delta = false);
    public uint GetUInt(bool delta = false);
    public bool GetBool(bool delta = false);
    public float GetFloat(bool delta = false);
    public short GetShort(bool delta = false);

    public byte[] GetBytes();
    public void SkipBytes(int count);
    public void Load(byte[] data);
    public void Reset();
}