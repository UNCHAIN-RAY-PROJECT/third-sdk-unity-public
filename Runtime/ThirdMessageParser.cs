using System;
using UnityEngine;

namespace UNCHAIN.ThirdSdk
{
    public enum ThirdInboundMessageType
    {
        Unknown,
        Ping,
        ActionExecuted
    }

    public sealed class ThirdParsedMessage
    {
        public ThirdInboundMessageType Type { get; }
        public ThirdResponse Response { get; }
        public string OutgoingMessage { get; }

        public ThirdParsedMessage(ThirdInboundMessageType type, ThirdResponse response, string outgoingMessage)
        {
            this.Type = type;
            this.Response = response;
            this.OutgoingMessage = outgoingMessage;
        }
    }

    public sealed class ThirdMessageParser
    {
        public bool TryParseToken(string json, out string accessToken)
        {
            accessToken = string.Empty;
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var tokenResponse = JsonUtility.FromJson<ThirdResponse_token>(json);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.accessToken))
            {
                return false;
            }

            accessToken = tokenResponse.accessToken;
            return true;
        }

        public ThirdParsedMessage ParseIncomingMessage(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new ThirdParsedMessage(ThirdInboundMessageType.Unknown, null, null);
            }

            var raw = JsonUtility.FromJson<ThirdResponse_root>(json);
            if (raw == null)
            {
                return new ThirdParsedMessage(ThirdInboundMessageType.Unknown, null, null);
            }

            switch (raw.type)
            {
                case "ping":
                    {
                        var ts = GetUnixTimeSeconds();
                        var pong = $"{{\"type\":\"pong\",\"data\":{{\"ts\":{ts}}}}}";
                        return new ThirdParsedMessage(ThirdInboundMessageType.Ping, null, pong);
                    }
                case "actionExecuted":
                    {
                        var txId = raw.data == null ? string.Empty : raw.data.txId;
                        var ack = $"{{\"type\":\"actionAck\",\"data\":{{\"txId\":\"{txId}\",\"status\":\"success\"}}}}";
                        return new ThirdParsedMessage(ThirdInboundMessageType.ActionExecuted, raw.data, ack);
                    }
                default:
                    return new ThirdParsedMessage(ThirdInboundMessageType.Unknown, null, null);
            }
        }

        private static uint GetUnixTimeSeconds()
        {
            var timespan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (uint)timespan.TotalSeconds;
        }
    }
}
