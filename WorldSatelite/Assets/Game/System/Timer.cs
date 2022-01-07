using System.Collections;
using UnityEngine;

namespace Game.System
{
    public abstract class Timer : MonoBehaviour
    {
        //1 tick == 10ms
        //6000 ticks == 1 minute
        private const int EarthRoundTrip = 8640000; //1 day in ticks
        private const int SatelliteRoundTrip = 540000; //ca. 90
        private const float TickerRate = 0.01f;
        private const int ScaleMinuteToShortTicker = 80; //how fast 1min should be scaled (in ticks)
        private WaitForSeconds waitForSeconds = new WaitForSeconds(TickerRate);

        protected const int TimeScale = 6000 / ScaleMinuteToShortTicker; //6000 / 80 = 75
        protected const float EarthRotationFactor = 360 / ((float)EarthRoundTrip / TimeScale); 
        protected const float SatelliteSpeedFactor = 60 / ((float)SatelliteRoundTrip / TimeScale);

        protected IEnumerator CheckTime()
        {
            yield return null;
            while (Application.isPlaying)
            {
                DoWork();
                yield return waitForSeconds;
            }
        }

        protected abstract void DoWork();
    }
}
