using System;
using System.Collections.Generic;
using UnityEngine;

public struct CompressionSchema
{
    public bool DeltaCompression;
    public bool DeltaZeroCompression;
    public bool DeltaZeroDeep;
    public bool SmallestThree;
    public bool BitPacking;
}

public class CompressionManager
{
    BitBuffer bitBuffer;
    ByteBuffer byteBuffer;
    Dictionary<int, Snapshot> snapshots;
    CompressionSchema compressionSchema;

    public CompressionManager()
    {
        bitBuffer = new BitBuffer(GameConfig.snapshotPacketSize);
        byteBuffer = new ByteBuffer(GameConfig.snapshotPacketSize);
        snapshots = new Dictionary<int, Snapshot>();
        compressionSchema = new CompressionSchema();
    }

    public byte[] EncodeSnapshot(Snapshot snapshot, int lastAckedCommandTick, int clientAckedTick)
    {
        snapshots.Add(snapshot.Tick, snapshot);
        if(snapshot.Tick % 20 == 0)
        {
            snapshots.RemoveLowerThan(clientAckedTick);
        }

        Snapshot _snapshot;
        if(GameConfig.deltaCompression && clientAckedTick != -1)
        {
            _snapshot = DeltaWriter.WriteDeltaSnapshot(snapshot, snapshots[clientAckedTick]);
        }
        else
        {
            _snapshot = snapshot;
        }

        INetBuffer writer;
        if (GameConfig.bitPacking)
        {
            writer = bitBuffer;
        }
        else
        {
            writer = byteBuffer;
        }
        writer.CompressionSchema = compressionSchema;
        writer.Reset();

        writer.Put((byte)MessageTypes.Snapshot);
        writer.Put(EncodeCompressionSchema());
        writer.Put(lastAckedCommandTick);

        int baselineTickMask = 0;
        if (clientAckedTick != -1)
        {
            baselineTickMask = snapshot.Tick - clientAckedTick;
        }

        writer.Put((byte)baselineTickMask);

        _snapshot.Serialize(writer);

        return writer.GetBytes();
    }

    public Snapshot DecodeSnapshot(byte[] data, out int lastReceivedCommandTick)
    {
        DecodeCompressionSchema(data[0]);

        INetBuffer reader;
        if (compressionSchema.BitPacking)
        {
            reader = bitBuffer;
        }
        else
        {
            reader = byteBuffer;
        }
        reader.CompressionSchema = compressionSchema;
        reader.Load(data);
        reader.SkipBytes(1);

        lastReceivedCommandTick = reader.GetInt();
        int baselineTickMask = reader.GetByte();

        var snapshot = new Snapshot();
        snapshot.Deserialize(reader);

        if(!compressionSchema.DeltaCompression || baselineTickMask == 0)
        {
            snapshots.Add(snapshot.Tick, snapshot);
            return snapshot;
        }

        int baselineTick = snapshot.Tick - baselineTickMask;
        snapshot = DeltaReader.ReadDeltaSnapshot(snapshot, snapshots[baselineTick]);

        if (snapshot.Tick % 20 == 0)
        {
            snapshots.RemoveLowerThan(baselineTick);
        }
        snapshots.Add(snapshot.Tick, snapshot);

        return snapshot;
    }

    byte EncodeCompressionSchema()
    {
        var schema = CompressonMethods.None;
        if (GameConfig.deltaCompression) schema |= CompressonMethods.Delta;
        if (GameConfig.deltaZeroCompression) schema |= CompressonMethods.DeltaZero;
        if (GameConfig.deltaZeroDeep) schema |= CompressonMethods.DeltaZeroDeep;
        if (GameConfig.smallestThree) schema |= CompressonMethods.SmallestThree;
        if (GameConfig.bitPacking) schema |= CompressonMethods.BitPacking;

        return (byte)schema;
    }

    void DecodeCompressionSchema(byte value)
    {
        var schema = (CompressonMethods)value;
        compressionSchema.DeltaCompression = (schema & CompressonMethods.Delta) != 0;
        compressionSchema.DeltaZeroCompression = (schema & CompressonMethods.DeltaZero) != 0;
        compressionSchema.DeltaZeroDeep = (schema & CompressonMethods.DeltaZeroDeep) != 0;
        compressionSchema.SmallestThree = (schema & CompressonMethods.SmallestThree) != 0;
        compressionSchema.BitPacking = (schema & CompressonMethods.BitPacking) != 0;
    }
}

