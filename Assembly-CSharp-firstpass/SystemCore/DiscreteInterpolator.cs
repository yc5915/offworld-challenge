using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace Offworld.SystemCore
{
    public class DiscreteInterpolator
    {
        private float lastTime = 0;
        private float lastValue = 0;
        private float currentRate = 0;
        private float currentValue = 0;
        private float timeToCatchUp = 1.0f; //1s to 

        public float LastTime { get { return lastTime; } }
        public float LastValue { get { return lastValue; } }
        public float CurrentRate { get { return currentRate; } }
        public float CurrentValue { get { return currentValue; } }
        public float TimeToCatchUp { get { return timeToCatchUp; } }

        //timeToCatchUp is how much time the currentValue is given to re-adjust to match the new target line
        public DiscreteInterpolator(float timeToCatchUp = 1.0f)
        {
            Assert.IsTrue(timeToCatchUp > MathUtilities.cEPSILON);
            this.timeToCatchUp = timeToCatchUp;
        }

        public void Reset(float initialTime, float initialValue, float initialRate)
        {
            lastTime = initialTime;
            lastValue = initialValue;
            currentRate = initialRate;
            currentValue = initialValue;
        }

        //only sets the new value if it's different than the last value, or force is set to true
        public void SetNextValue(float time, float value, bool force = false)
        {
            if(force || !MathUtilities.EpsilonEquals(lastValue, value))
            {
                float deltaTime = time - lastTime;
                float deltaValue = value - lastValue;
                float targetRate = deltaValue / Mathf.Max(deltaTime, MathUtilities.cEPSILON);
                float extrapolatedValue = value + targetRate * timeToCatchUp; //predict where we need to be 1 second from now

                lastTime = time;
                lastValue = value;
                currentRate = (extrapolatedValue - currentValue) / timeToCatchUp;
            }
        }

        //only sets the new value if it's different than the last value, or force is set to true
        public void SetNextValueRate(float time, float value, float rate, bool force = false)
        {
            if(force || !MathUtilities.EpsilonEquals(lastValue, value))
            {
                float extrapolatedValue = value + rate * timeToCatchUp; //predict where we need to be 1 second from now

                lastTime = time;
                lastValue = value;
                currentRate = (extrapolatedValue - currentValue) / timeToCatchUp;
            }
        }

        public void Update(float deltaTime)
        {
            currentValue += currentRate * deltaTime;
        }
    }
}