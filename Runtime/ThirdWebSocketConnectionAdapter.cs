using System;
using MikeSchweitzer.WebSocket;
using UnityEngine;

namespace UNCHAIN.ThirdSdk
{
    [DisallowMultipleComponent]
    public sealed class ThirdWebSocketConnectionAdapter : MonoBehaviour, IThirdWebSocketClient
    {
        private WebSocketConnection connection;

        public event Action<ThirdWebSocketState, ThirdWebSocketState> StateChanged;
        public event Action<string> MessageReceived;
        public event Action<string> ErrorMessageReceived;

        public ThirdWebSocketState State => this.connection == null
            ? ThirdWebSocketState.Disconnected
            : ConvertState(this.connection.State);

        private void Awake()
        {
            this.connection = this.GetComponent<WebSocketConnection>();
            if (this.connection == null)
            {
                this.connection = this.gameObject.AddComponent<WebSocketConnection>();
            }

            this.connection.StateChanged += this.OnStateChanged;
            this.connection.MessageReceived += this.OnMessageReceived;
            this.connection.ErrorMessageReceived += this.OnErrorMessageReceived;
        }

        private void OnDestroy()
        {
            if (this.connection == null)
            {
                return;
            }

            this.connection.StateChanged -= this.OnStateChanged;
            this.connection.MessageReceived -= this.OnMessageReceived;
            this.connection.ErrorMessageReceived -= this.OnErrorMessageReceived;
        }

        public void Configure(string url)
        {
            this.connection.DesiredConfig = new WebSocketConfig { Url = url };
        }

        public void Connect()
        {
            this.connection.Connect();
        }

        public void Disconnect()
        {
            this.connection.Disconnect();
        }

        public void Send(string message)
        {
            this.connection.AddOutgoingMessage(message);
        }

        private void OnStateChanged(WebSocketConnection _, WebSocketState oldState, WebSocketState newState)
        {
            this.StateChanged?.Invoke(ConvertState(oldState), ConvertState(newState));
        }

        private void OnMessageReceived(WebSocketConnection _, WebSocketMessage message)
        {
            this.MessageReceived?.Invoke(message.String);
        }

        private void OnErrorMessageReceived(WebSocketConnection _, string errorMessage)
        {
            this.ErrorMessageReceived?.Invoke(errorMessage);
        }

        private static ThirdWebSocketState ConvertState(WebSocketState state)
        {
            return state switch
            {
                WebSocketState.Connected => ThirdWebSocketState.Connected,
                WebSocketState.Connecting => ThirdWebSocketState.Connecting,
                WebSocketState.Disconnecting => ThirdWebSocketState.Disconnecting,
                _ => ThirdWebSocketState.Disconnected
            };
        }
    }
}
