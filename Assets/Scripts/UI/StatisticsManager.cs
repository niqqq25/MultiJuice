using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager instance;

    public Text bandwidthText;

    public int ReceivedBytes = 0;
    float elapsedBandwidthTime = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SetBandwidth();
    }

    void Update()
    {
        elapsedBandwidthTime += Time.deltaTime;
        if (elapsedBandwidthTime >= 1f)
        {
            SetBandwidth();
            elapsedBandwidthTime = 0;
        }
    }

    void SetBandwidth()
    {
        bandwidthText.text = $"Bandwidth: {SizeSuffix(ReceivedBytes * 6 * 8, 2)}";
        ReceivedBytes = 0;
    }

    string[] SizeSuffixes = { "bps", "Kbps", "Mbps", "Gbps" };
    string SizeSuffix(int value, int decimalPlaces = 1)
    {
        if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
        if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bps", 0); }

        int mag = (int)Math.Log(value, 1024);

        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }
}
