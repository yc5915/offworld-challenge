using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class PoolManager
    {
        private ResetPool<StringBuilder> stringBuilderPool;

        public PoolManager()
        {
            stringBuilderPool = CreateStringBuilderPool();
        }

        public void Update()
        {
            stringBuilderPool.Reset();
        }

        // StringBuilder
        public StringBuilder AcquireScratchStringBuilder()
        {
            return stringBuilderPool.Acquire();
        }

        private static ResetPool<StringBuilder> CreateStringBuilderPool()
        {
            return new ResetPool<StringBuilder>(() => new StringBuilder(), t => t.Clear(), null, null);
        }
    }
}