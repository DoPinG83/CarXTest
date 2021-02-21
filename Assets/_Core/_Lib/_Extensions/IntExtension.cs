namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;

    public static class IntExtension {
        public static void Every(this int count, Action<int> action) {
            for (int i = 0; i < count; i++) action(i);
        }

        public static int RandomInRange(this int max) {
            return UnityEngine.Random.Range(0, max + 1);
        }
    }
}