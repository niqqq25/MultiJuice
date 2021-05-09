using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolsUI : MonoBehaviour
{
    public Toggle showServerToggle;
    public Toggle showServerRayToggle;
    public Toggle serverReconcilationToggle;
    public Toggle extrapolationToggle;
    public Toggle entityInterpolationToggle;
    public Toggle lagCompensationToggle;
    public Toggle fakePlayersToggle;
    public Toggle smallestThreeToggle;
    public Toggle deltaCompressionToggle;
    public Toggle deltaZeroToggle;
    public Toggle deltaZeroDeepToggle;
    public Toggle bitPackingToggle;

    public InputField playerCountInput;
    public InputField movingPlayerCountInput;

    void Start()
    {
        showServerToggle.onValueChanged.AddListener(delegate {
            HandleShowServer(showServerToggle);
        });
        HandleShowServer(showServerToggle);

        showServerRayToggle.onValueChanged.AddListener(delegate {
            HandleShowServerRay(showServerRayToggle);
        });
        HandleShowServerRay(showServerRayToggle);

        serverReconcilationToggle.onValueChanged.AddListener(delegate {
            HandleServerReconsilation(serverReconcilationToggle);
        });
        HandleServerReconsilation(serverReconcilationToggle);

        entityInterpolationToggle.onValueChanged.AddListener(delegate {
            HandleEntityInterpolation(entityInterpolationToggle);
        });
        HandleEntityInterpolation(entityInterpolationToggle);

        extrapolationToggle.onValueChanged.AddListener(delegate
        {
            HandleExtrapolation(extrapolationToggle);
        });
        HandleExtrapolation(extrapolationToggle);

        lagCompensationToggle.onValueChanged.AddListener(delegate {
            HandleLagCompensation(lagCompensationToggle);
        });
        HandleLagCompensation(lagCompensationToggle);

        fakePlayersToggle.onValueChanged.AddListener(delegate {
            HandleFakePlayers(fakePlayersToggle);
        });
        HandleFakePlayers(fakePlayersToggle);

        smallestThreeToggle.onValueChanged.AddListener(delegate {
            HandleSmallestThree(smallestThreeToggle);
        });
        HandleSmallestThree(smallestThreeToggle);

        deltaCompressionToggle.onValueChanged.AddListener(delegate {
            HandleDeltaCompression(deltaCompressionToggle);
        });
        HandleDeltaCompression(deltaCompressionToggle);

        deltaZeroToggle.onValueChanged.AddListener(delegate {
            HandleDeltaZero(deltaZeroToggle);
        });
        HandleDeltaZero(deltaZeroToggle);

        deltaZeroDeepToggle.onValueChanged.AddListener(delegate {
            HandleDeltaZeroDeep(deltaZeroDeepToggle);
        });
        HandleDeltaZeroDeep(deltaZeroDeepToggle);

        bitPackingToggle.onValueChanged.AddListener(delegate {
            HandleBitPacking(bitPackingToggle);
        });
        HandleBitPacking(bitPackingToggle);

        if(GameConfig.isClientOnly)
        {
            showServerToggle.enabled = false;
            showServerRayToggle.enabled = false;
            lagCompensationToggle.enabled = false;
            fakePlayersToggle.enabled = false;
            smallestThreeToggle.enabled = false;
            deltaCompressionToggle.enabled = false;
            deltaZeroToggle.enabled = false;
            deltaZeroDeepToggle.enabled = false;
            bitPackingToggle.enabled = false;

            playerCountInput.enabled = false;
            movingPlayerCountInput.enabled = false;
        }
    }

    void HandleShowServer(Toggle toggle)
    {
        GameObject[] serverPlayers = GameObject.FindGameObjectsWithTag("ServerPlayer");
        GameConfig.showServerPlayers = toggle.isOn;

        float opacity;
        if (toggle.isOn)
        {
            opacity = 1;
        }
        else
        {
            opacity = 0;
        }

        foreach (var serverPlayer in serverPlayers)
        {
            Utils.ChangeObjectOpacity(serverPlayer.transform, opacity);
        }
    }

    void HandleShowServerRay(Toggle toggle)
    {
        GameConfig.showServerRay = toggle.isOn;
    }

    void HandleServerReconsilation(Toggle toggle)
    {
        GameConfig.userReconsilation = toggle.isOn;
    }

    void HandleExtrapolation(Toggle toggle)
    {
        if(toggle.isOn)
        {
            ExtrapolationManager.instance.Reset();
            GameConfig.extrapolation = true;
        }
        else
        {
            GameConfig.extrapolation = false;
        }
    }

    void HandleEntityInterpolation(Toggle toggle)
    {
        if (toggle.isOn)
        {
            InterpolationManager.instance.Reset();
            GameConfig.entityInterpolation = true;
        }
        else
        {
            GameConfig.entityInterpolation = false;
        }
    }

    void HandleLagCompensation(Toggle toggle)
    {
        GameConfig.lagCompensation = toggle.isOn;
    }

    void HandleFakePlayers(Toggle toggle)
    {
        //if (toggle.isOn)
        //{
        //    int.TryParse(playerCountInput.text, out int playerCount);
        //    int.TryParse(movingPlayerCountInput.text, out int movingPlayerCount);
        //    playerCount = playerCount != 0 ? playerCount : 50;
        //    movingPlayerCount = movingPlayerCount != 0 ? movingPlayerCount : 20;

        //    Server.instance.AddFakePlayers(playerCount, movingPlayerCount);
        //}
        //else
        //{
        //    Server.instance.RemoveFakePlayers();
        //}

        playerCountInput.enabled = !toggle.isOn;
        movingPlayerCountInput.enabled = !toggle.isOn;
    }

    void HandleSmallestThree(Toggle toggle)
    {
        GameConfig.smallestThree = toggle.isOn;
    }

    void HandleDeltaCompression(Toggle toggle)
    {
        GameConfig.deltaCompression = toggle.isOn;
    }

    void HandleDeltaZero(Toggle toggle)
    {
        GameConfig.deltaZeroCompression = toggle.isOn;
    }

    void HandleDeltaZeroDeep(Toggle toggle)
    {
        GameConfig.deltaZeroDeep = toggle.isOn;
    }

    void HandleBitPacking(Toggle toggle)
    {
        GameConfig.bitPacking = toggle.isOn;
    }
}
