using NUnit.Framework;
using System;
using System.Collections;
using UNCHAIN.ThirdSdk;
using UnityEngine;
using UnityEngine.TestTools;

namespace UNCHAIN.ThirdSdk.Tests.EditMode
{
    public class ThirdConnectorTestabilityTests
    {
        [Test]
        public void SetWebSocketClient_InjectsMockClient()
        {
            var gameObject = new GameObject();
            var connector = gameObject.AddComponent<ThirdConnector>();
            var mockClient = new MockWebSocketClient();

            connector.SetWebSocketClient(mockClient);

            Assert.DoesNotThrow(() => mockClient.EmitMessage("{\"type\":\"ping\",\"data\":{}}"));
            Object.DestroyImmediate(gameObject);
        }

        [UnityTest]
        public IEnumerator Connect_UsesFactoryToCreateClient()
        {
            var gameObject = new GameObject();
            var connector = gameObject.AddComponent<ThirdConnector>();
            connector.url = "http://127.0.0.1:9";
            connector.appId = "app";
            connector.apiKey = "key";

            var factoryCalled = false;
            connector.WebSocketClientFactory = _ =>
            {
                factoryCalled = true;
                return new MockWebSocketClient();
            };

            yield return connector.Connect("stream");

            Assert.IsFalse(factoryCalled, "Token取得失敗時はWebSocketクライアントを生成しない");
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void ThirdMessageParser_CreatesAckForActionExecuted()
        {
            var parser = new ThirdMessageParser();
            var json = "{\"type\":\"actionExecuted\",\"data\":{\"txId\":\"tx-1\"}}";

            var parsed = parser.ParseIncomingMessage(json);

            Assert.AreEqual(ThirdInboundMessageType.ActionExecuted, parsed.Type);
            Assert.NotNull(parsed.Response);
            Assert.That(parsed.OutgoingMessage, Does.Contain("actionAck"));
            Assert.That(parsed.OutgoingMessage, Does.Contain("tx-1"));
        }

        private sealed class MockWebSocketClient : IThirdWebSocketClient
        {
            public event Action<ThirdWebSocketState, ThirdWebSocketState> StateChanged;
            public event Action<string> MessageReceived;
            public event Action<string> ErrorMessageReceived;

            public ThirdWebSocketState State { get; private set; } = ThirdWebSocketState.Disconnected;

            public void Configure(string url)
            {
            }

            public void Connect()
            {
                var previous = this.State;
                this.State = ThirdWebSocketState.Connected;
                this.StateChanged?.Invoke(previous, this.State);
            }

            public void Disconnect()
            {
                var previous = this.State;
                this.State = ThirdWebSocketState.Disconnected;
                this.StateChanged?.Invoke(previous, this.State);
            }

            public void Send(string message)
            {
            }

            public void EmitMessage(string message)
            {
                this.MessageReceived?.Invoke(message);
            }

            public void EmitError(string error)
            {
                this.ErrorMessageReceived?.Invoke(error);
            }
        }
    }
}
