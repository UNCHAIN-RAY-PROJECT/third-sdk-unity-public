using NUnit.Framework;
using UNCHAIN.ThirdSdk;
using UnityEngine;

namespace UNCHAIN.ThirdSdk.Tests.EditMode
{
    public class ThirdConnectorSmokeTests
    {
        [Test]
        public void ThirdConnectorCanBeAddedToGameObject()
        {
            var gameObject = new GameObject();

            var connector = gameObject.AddComponent<ThirdConnector>();

            Assert.NotNull(connector);
            Object.DestroyImmediate(gameObject);
        }
    }
}
