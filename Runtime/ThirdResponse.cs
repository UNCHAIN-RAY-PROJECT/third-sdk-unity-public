using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using MikeSchweitzer.WebSocket;

namespace UNCHAIN.ThirdSdk
{
    [System.Serializable]
    public sealed class ThirdResponse_token
    {
        public string accessToken;
    }

    [System.Serializable]
    public sealed class ThirdResponse_root
    {
        public string type;
        public ThirdResponse data;
    }

    [System.Serializable]
    public sealed class ThirdResponse
    {
        public string txId;
        public string streamId;
        public string actionId;
        public string quantity;
        public string commandKey;
        public string displayName;
    }
}
