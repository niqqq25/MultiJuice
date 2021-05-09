
public static class GameConfig
{
    // can be changed
    public static bool showServerPlayers = false;
    public static bool showServerRay = false;
    public static bool userReconsilation = false;
    public static bool extrapolation = false;
    public static bool entityInterpolation = false;
    public static bool lagCompensation = false;

    public static bool smallestThree = false;
    public static bool deltaCompression = false;
    public static bool deltaZeroCompression = false;
    public static bool deltaZeroDeep = false;
    public static bool bitPacking = false;

    // cannot be changed
    public const int serverTickRate = 10;
    public const int clientTickRate = 50;
    public const int serverPort = 9999;
    public const int clientTimeout = 1000;
    public const int serverTimeout = 1000;
    public const int snapshotPacketSize = 60000;
    public const int maxClients = 16;
    public const bool isClientOnly = true;
}
