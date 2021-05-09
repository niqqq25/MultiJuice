using System;
using LiteNetLib.Utils;
using UnityEngine;

public struct Buttons
{
    public bool Up;
    public bool Down;
    public bool Left;
    public bool Right;
    public bool Fire;

    public void Serialize(NetDataWriter writer)
    {
        var buttons = ButtonTypes.None;
        if (Up) buttons |= ButtonTypes.Up;
        if (Down) buttons |= ButtonTypes.Down;
        if (Left) buttons |= ButtonTypes.Left;
        if (Right) buttons |= ButtonTypes.Right;
        if (Fire) buttons |= ButtonTypes.Fire;

        writer.Put((byte)buttons);
    }

    public void Deserialize(NetDataReader reader)
    {
        var buttons = (ButtonTypes)reader.GetByte();
        Up = (buttons & ButtonTypes.Up) != 0;
        Down = (buttons & ButtonTypes.Down) != 0;
        Left = (buttons & ButtonTypes.Left) != 0;
        Right = (buttons & ButtonTypes.Right) != 0;
        Fire = (buttons & ButtonTypes.Fire) != 0;
    }
}

public struct UserCommand
{
    public Buttons Buttons;
    public Quaternion Rotation;

    public void Serialize(NetDataWriter writer)
    {
        Buttons.Serialize(writer);
        writer.Put(Rotation);
    }

    public void Deserialize(NetDataReader reader)
    {
        Buttons.Deserialize(reader);
        Rotation = reader.GetQuaternion();
    }
}