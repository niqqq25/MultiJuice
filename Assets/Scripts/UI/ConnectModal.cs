using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class ConnectModal : MonoBehaviour
{
    public GameObject connectModal;

    public InputField hostnameInput;
    public Text connectButtonText;
    public Text errorText;

    public static ConnectModal instance;

    private void Awake()
    {
        instance = this;
    }

    public void HandleConnect()
    {
        IPEndPoint endPoint = new IPEndPoint(0, 0);

        if(string.IsNullOrEmpty(hostnameInput.text))
        {
            Client.instance.Connect("127.0.0.1", GameConfig.serverPort);
            return;
        }

        if(endPoint.TryParse(hostnameInput.text, out endPoint))
        {
            Client.instance.Connect(endPoint.Address.ToString(), endPoint.Port);
        } else
        {
            errorText.text = "Wrong format";
        }
    }

    public void ChangeToConnectingState()
    {
        connectButtonText.text = "Connecting...";
        errorText.text = "";
    }

    public void ChangeToBrowsingState(string reason)
    {
        connectButtonText.text = "Connect";
        errorText.text = reason;
    }

    public void Close()
    {
        connectModal.SetActive(false);
    }
}
