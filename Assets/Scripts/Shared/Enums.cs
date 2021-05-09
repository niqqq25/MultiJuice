
public enum MessageTypes
{
    ClientAccepted,
    Snapshot,
    ClientCommand
}

public enum CompressonMethods: byte
{
    None = 0,
    Delta = 1 << 0,
    DeltaZero = 1 << 1,
    SmallestThree = 1 << 2,
    BitPacking = 1 << 3,
    DeltaZeroDeep = 1 << 4
}

public enum ButtonTypes: byte
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,
    Fire = 1 << 4
}