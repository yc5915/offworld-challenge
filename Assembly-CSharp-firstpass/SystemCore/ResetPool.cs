using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

namespace Offworld.SystemCore
{
    public class ResetPool<T>
    {
        private int warningAllocations = 1000;
        private int totalAllocations = 0;

        private List<T> data = new List<T>();
        private int nextIndex = 0;
        private int postResetIndex = 0;
        private Func<T> allocateFunc;
        private Action<T> acquireFunc;
        private Action<T> resetFunc;
        private Action<T> postResetFunc;

        public int AcquiredCount { get { return nextIndex; } }
        public int WarningAllocations
        { 
            get { return warningAllocations; }
            set { warningAllocations = value; }
        }

        public ResetPool(Func<T> allocateFunc, Action<T> acquireFunc, Action<T> resetFunc, Action<T> postResetFunc)
        {
            Assert.IsNotNull(allocateFunc);
            this.allocateFunc = allocateFunc;
            this.acquireFunc = acquireFunc;
            this.resetFunc = resetFunc;
            this.postResetFunc = postResetFunc;
        }

        public void Reset()
        {
            if (resetFunc != null)
            {
                for(int i=0; i<nextIndex; i++)
                    resetFunc(data[i]);
            }
            PostReset();
            nextIndex = 0;
        }

        public void PostReset()
        {
            if (postResetFunc != null)
            {
                for(int i=nextIndex; i<postResetIndex; i++)
                    postResetFunc(data[i]);
            }
            postResetIndex = nextIndex;
        }

        public T Acquire()
        {
            if(nextIndex == data.Count)
                data.Add(NewAllocation());

            T result = data[nextIndex++];
            if(acquireFunc != null)
                acquireFunc(result);

            return result;
        }

        private T NewAllocation()
        {
            T result = allocateFunc();
            
            //sanity check
            totalAllocations++;
            if(totalAllocations == warningAllocations)
                Debug.LogWarning("Over " + warningAllocations + " pool allocations for " + typeof(T));
            
            return result;
        }
    }
}