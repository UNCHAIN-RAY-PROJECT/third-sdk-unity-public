using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using MikeSchweitzer.WebSocket;

namespace UNCHAIN.ThirdSdk
{
    public class ThirdConnector : MonoBehaviour
    {
        public string url = "https://live-ctl.com";
        public string wsurl = "wss://live-ctl.com";
        public string appId;
        public string apiKey;

        public UnityEvent Connected;
        public UnityEvent Disconnected;
        public UnityEvent<ThirdResponse> MessageReceived;
        public UnityEvent<string> ErrorMessageReceived;

        private WebSocketConnection con;
        private Coroutine recon;

        private void OnDestroy()
        {
            if (this.con != null)
            {
                this.con.Disconnect();
                this.con = null;
            }
        }

        public IEnumerator Connect(string streamId)
        {
            if (!this.IsValid())
            {
                yield break;
            }
            if (string.IsNullOrEmpty(streamId))
            {
                Debug.Log("[THIRD] streamId is empty.");
                yield break;
            }

            var url = this.url;
            var req = UnityWebRequest.PostWwwForm($"{url}/api/v1/auth/ws-token", "POST");
            req.SetRequestHeader("Content-Type", "application/json");

            var json = $"{{\"appId\":\"{appId}\",\"apiKey\":\"{apiKey}\",\"streamCode\":\"{streamId}\"}}";
            var postData = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(postData);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("[THIRD] get token failed.");
                Debug.Log(req.error);
                yield break;
            }

            var token = "";
            try
            {
                var raw = JsonUtility.FromJson<ThirdResponse_token>(req.downloadHandler.text);
                token = raw.accessToken;
            }
            catch (System.Exception e)
            {
                Debug.Log("[THIRD] get token failed.");
                Debug.Log(e);
                yield break;
            }

            var wsurl = $"{this.wsurl}/api/game?streamCode={streamId}&token={token}";
            this.con = this.gameObject.AddComponent<WebSocketConnection>();
            this.con.DesiredConfig = new WebSocketConfig { Url = wsurl };
            this.con.Connect();
            this.con.StateChanged += OnStateChanged;
            this.con.MessageReceived += OnMessageReceived;
            this.con.ErrorMessageReceived += OnErrorMessageReceived;
            yield break;
        }

        public void Disconnect()
        {
            if (this.con == null)
            {
                Debug.Log("[THIRD] not connected.");
                return;
            }
            this.con.Disconnect();
            this.con = null;
        }

        public IEnumerator Reconnect()
        {
            if (this.con == null)
            {
                Debug.Log("[THIRD] not connected.");
                yield break;
            }
            this.con.Disconnect();
            yield return new WaitForSeconds(5.0f);
            yield return new WaitUntil(() => this.con.State == WebSocketState.Disconnected);
            this.con.Connect();
        }

        private void OnStateChanged(WebSocketConnection connection, WebSocketState oldState, WebSocketState newState)
        {
            Debug.Log($"[THIRD] WebSocket state changed from {oldState} to {newState}");
            switch (newState)
            {
                case WebSocketState.Connected:
                    {
                        this.Connected.Invoke();
                    }
                    break;
                case WebSocketState.Disconnected:
                    {
                        this.Disconnected.Invoke();
                        if (this.recon != null)
                        {
                            this.StopCoroutine(this.recon);
                            this.recon = null;
                        }
                        this.recon = this.StartCoroutine(this.Reconnect());
                    }
                    break;
            }
        }

        private void OnMessageReceived(WebSocketConnection connection, WebSocketMessage message)
        {
            Debug.Log($"[THIRD] Message received from server: {message.String}");
            var raw = JsonUtility.FromJson<ThirdResponse_root>(message.String);
            switch (raw.type)
            {
                case "ping":
                    {
                        var timespan = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                        var ts = (uint)timespan.TotalSeconds;
                        SendMessageToServer($"{{\"type\":\"pong\",\"data\":{{\"ts\":{ts}}}}}");
                    }
                    break;
                case "actionExecuted":
                    {
                        var txId = raw.data.txId;
                        SendMessageToServer($"{{\"type\":\"actionAck\",\"data\":{{\"txId\":\"{txId}\",\"status\":\"success\"}}}}");
                        this.MessageReceived.Invoke(raw.data);
                    }
                    break;
            }
        }

        private void OnErrorMessageReceived(WebSocketConnection connection, string errorMessage)
        {
            Debug.LogError($"[THIRD] WebSocket error: {errorMessage}");
            this.ErrorMessageReceived.Invoke(errorMessage);
        }

        private void SendMessageToServer(string message)
        {
            if (this.con != null && this.con.State == WebSocketState.Connected)
            {
                Debug.Log($"[THIRD] Message sent to server: {message}");
                this.con.AddOutgoingMessage(message);
            }
        }

        private bool IsValid()
        {
            if (string.IsNullOrEmpty(this.url))
            {
                Debug.Log("[THIRD] url is empty.");
                return false;
            }
            if (string.IsNullOrEmpty(this.appId))
            {
                Debug.Log("[THIRD] appId is empty.");
                return false;
            }
            if (string.IsNullOrEmpty(this.apiKey))
            {
                Debug.Log("[THIRD] apiKey is empty.");
                return false;
            }
            if (this.con != null)
            {
                Debug.Log("[THIRD] already connected.");
                return false;
            }
            return true;
        }
    }
}
