using System;

public class PlayerId
{
    int currentPlayerId = -1;

    public int GetNext()
    {
        currentPlayerId++;
        return currentPlayerId;
    }
}
