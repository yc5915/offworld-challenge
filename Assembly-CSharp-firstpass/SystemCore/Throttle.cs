using UnityEngine.Assertions;

namespace Offworld.SystemCore
{
    public class Throttle
    {
        private bool firstSignal;
        private float lastSignalTime;
        private float signalTimeInterval;

        public Throttle(float timeInterval)
        {
            signalTimeInterval = timeInterval;
            firstSignal = true;
        }

        //returns TRUE only if timeInterval time has passed since the last TRUE
        //typically usage: if(throttle.Check(Time.unscaledTime)) DoSomething();
        public bool Check(float currentTime)
        {
            if(firstSignal || (currentTime - lastSignalTime >= signalTimeInterval))
            {
                firstSignal = false;
                lastSignalTime = currentTime;
                return true; //signal
            }

            return false; //don't signal
        }
    }
}