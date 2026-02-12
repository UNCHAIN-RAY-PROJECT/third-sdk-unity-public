using System;

namespace UNCHAIN.ThirdSdk
{
    public enum ThirdWebSocketState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting
    }

    public interface IThirdWebSocketClient
    {
        event Action<ThirdWebSocketState, ThirdWebSocketState> StateChanged;
        event Action<string> MessageReceived;
        event Action<string> ErrorMessageReceived;

        ThirdWebSocketState State { get; }

        void Configure(string url);
        void Connect();
        void Disconnect();
        void Send(string message);
    }
}
