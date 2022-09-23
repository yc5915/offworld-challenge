using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

namespace Offworld.SystemCore
{
    public class ReleasePool<T>
    {
        private const int cWARNING_ALLOCATIONS = 1000;
        private int totalAllocations = 0;

        private Stack<T> freeObjects = new Stack<T>();
        private Func<T> allocateFunc;
        private Action<T> acquireFunc;
        private Action<T> releaseFunc;

        public ReleasePool(Func<T> allocateFunc, Action<T> acquireFunc, Action<T> releaseFunc)
        {
            Assert.IsNotNull(allocateFunc);
            this.allocateFunc = allocateFunc;
            this.acquireFunc = acquireFunc;
            this.releaseFunc = releaseFunc;
        }

        public T Acquire()
        {
            T result = (freeObjects.Count > 0) ? freeObjects.Pop() : NewAllocation();

            if (acquireFunc != null)
                acquireFunc(result);

            return result;
        }

        private T NewAllocation()
        {
            T result = allocateFunc();

            //sanity check
            totalAllocations++;
            if(totalAllocations == cWARNING_ALLOCATIONS)
                Debug.LogWarning("Over " + cWARNING_ALLOCATIONS + " pool allocations for " + typeof(T));

            return result;
        }

        public void Release(ref T value)
        {
            if (value != null)
            {
                if(releaseFunc != null)
                    releaseFunc(value);

                freeObjects.Push(value);
                value = default(T); //assigns null
            }
        }
    }
}