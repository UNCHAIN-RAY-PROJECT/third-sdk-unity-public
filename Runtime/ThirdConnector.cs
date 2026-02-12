using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

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

        private readonly ThirdMessageParser parser = new ThirdMessageParser();
        private IThirdWebSocketClient client;
        private Coroutine recon;

        public Func<ThirdConnector, IThirdWebSocketClient> WebSocketClientFactory { get; set; }

        private void OnDestroy()
        {
            this.UnsubscribeClientEvents();
            this.client?.Disconnect();
            this.client = null;
        }

        public void SetWebSocketClient(IThirdWebSocketClient webSocketClient)
        {
            this.UnsubscribeClientEvents();
            this.client = webSocketClient;
            this.SubscribeClientEvents();
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

            var request = UnityWebRequest.PostWwwForm($"{this.url}/api/v1/auth/ws-token", "POST");
            request.SetRequestHeader("Content-Type", "application/json");

            var json = $"{{\"appId\":\"{this.appId}\",\"apiKey\":\"{this.apiKey}\",\"streamCode\":\"{streamId}\"}}";
            var postData = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(postData);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("[THIRD] get token failed.");
                Debug.Log(request.error);
                yield break;
            }

            if (!this.parser.TryParseToken(request.downloadHandler.text, out var token))
            {
                Debug.Log("[THIRD] get token failed.");
                yield break;
            }

            var webSocketUrl = $"{this.wsurl}/api/game?streamCode={streamId}&token={token}";
            this.EnsureClient();
            this.client.Configure(webSocketUrl);
            this.client.Connect();
        }

        public void Disconnect()
        {
            if (this.client == null)
            {
                Debug.Log("[THIRD] not connected.");
                return;
            }

            this.client.Disconnect();
            this.client = null;
        }

        public IEnumerator Reconnect()
        {
            if (this.client == null)
            {
                Debug.Log("[THIRD] not connected.");
                yield break;
            }

            this.client.Disconnect();
            yield return new WaitForSeconds(5.0f);
            yield return new WaitUntil(() => this.client != null && this.client.State == ThirdWebSocketState.Disconnected);
            this.client.Connect();
        }

        private void OnStateChanged(ThirdWebSocketState oldState, ThirdWebSocketState newState)
        {
            Debug.Log($"[THIRD] WebSocket state changed from {oldState} to {newState}");

            switch (newState)
            {
                case ThirdWebSocketState.Connected:
                    this.Connected.Invoke();
                    break;
                case ThirdWebSocketState.Disconnected:
                    this.Disconnected.Invoke();
                    if (this.recon != null)
                    {
                        this.StopCoroutine(this.recon);
                        this.recon = null;
                    }

                    this.recon = this.StartCoroutine(this.Reconnect());
                    break;
            }
        }

        private void OnMessageReceived(string message)
        {
            Debug.Log($"[THIRD] Message received from server: {message}");
            var parsed = this.parser.ParseIncomingMessage(message);

            if (!string.IsNullOrEmpty(parsed.OutgoingMessage))
            {
                this.SendMessageToServer(parsed.OutgoingMessage);
            }

            if (parsed.Type == ThirdInboundMessageType.ActionExecuted && parsed.Response != null)
            {
                this.MessageReceived.Invoke(parsed.Response);
            }
        }

        private void OnErrorMessageReceived(string errorMessage)
        {
            Debug.LogError($"[THIRD] WebSocket error: {errorMessage}");
            this.ErrorMessageReceived.Invoke(errorMessage);
        }

        private void SendMessageToServer(string message)
        {
            if (this.client != null && this.client.State == ThirdWebSocketState.Connected)
            {
                Debug.Log($"[THIRD] Message sent to server: {message}");
                this.client.Send(message);
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

            if (this.client != null)
            {
                Debug.Log("[THIRD] already connected.");
                return false;
            }

            return true;
        }

        private void EnsureClient()
        {
            if (this.client != null)
            {
                return;
            }

            if (this.WebSocketClientFactory != null)
            {
                this.client = this.WebSocketClientFactory(this);
            }
            else
            {
                this.client = this.gameObject.AddComponent<ThirdWebSocketConnectionAdapter>();
            }

            this.SubscribeClientEvents();
        }

        private void SubscribeClientEvents()
        {
            if (this.client == null)
            {
                return;
            }

            this.client.StateChanged += this.OnStateChanged;
            this.client.MessageReceived += this.OnMessageReceived;
            this.client.ErrorMessageReceived += this.OnErrorMessageReceived;
        }

        private void UnsubscribeClientEvents()
        {
            if (this.client == null)
            {
                return;
            }

            this.client.StateChanged -= this.OnStateChanged;
            this.client.MessageReceived -= this.OnMessageReceived;
            this.client.ErrorMessageReceived -= this.OnErrorMessageReceived;
        }
    }
}
