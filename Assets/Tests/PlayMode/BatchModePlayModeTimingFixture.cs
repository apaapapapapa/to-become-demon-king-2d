using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.PlayMode
{
    /// <summary>
    /// BatchModeでは描画待ちがなくフレームが極端に短くなるため、
    /// フレーム進行を前提とするPlayModeテストのdeltaTimeを固定します。
    /// </summary>
    [SetUpFixture]
    public sealed class BatchModePlayModeTimingFixture
    {
        private int previousCaptureFramerate;

        [OneTimeSetUp]
        public void SetUpBatchModeTiming()
        {
            previousCaptureFramerate = Time.captureFramerate;
            if (Application.isBatchMode)
            {
                Time.captureFramerate = 60;
            }
        }

        [OneTimeTearDown]
        public void RestoreBatchModeTiming()
        {
            Time.captureFramerate = previousCaptureFramerate;
        }
    }
}
