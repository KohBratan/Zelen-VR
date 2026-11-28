using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Zelen.Tests.PlayMode
{
    public class MainMenuSceneTests
    {
        [UnityTest]
        public IEnumerator PlayModeStarts()
        {
            yield return null;
            Assert.IsTrue(Application.isPlaying, "Play mode did not start");
        }

        [Test]
        public void TimeIsAvailable()
        {
            Assert.GreaterOrEqual(Time.time, 0f);
        }
    }
}
