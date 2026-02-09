using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNCHAIN.ThirdSdk;

public class Sample : MonoBehaviour
{
    public ThirdConnector ThirdConnector;
    public UnityEngine.UI.InputField inputField;

    public void Connect()
    {
        var streamId = this.inputField.text;
        this.StartCoroutine(this.ConnectCoroutine(streamId));
    }

    private IEnumerator ConnectCoroutine(string streamId)
    {
        yield return this.ThirdConnector.Connect(streamId);
    }

    public void Disconnect()
    {
        this.ThirdConnector.Disconnect();
    }

    public void OnConnected()
    {
        Debug.Log("connected.");
    }

    public void OnDisconnected()
    {
        Debug.Log("disconnected.");
    }

    public void OnMessageReceived(ThirdResponse data)
    {
        Debug.Log($"{data.txId}, {data.streamId}, {data.actionId}, {data.quantity}, {data.commandKey}, {data.displayName}");
    }

    public void OnErrorMessageReceived(string message)
    {
        Debug.Log(message);
    }
}
