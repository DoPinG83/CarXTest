namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public static class Vector2Extension {
        public static bool ApproxEq(this Vector2 a, Vector2 b, float sigma) {
            return Mathf.Abs(a.x - b.x) <= sigma && Mathf.Abs(a.y - b.y) <= sigma;
        }

        public static float RandomInRange(this Vector2 range) {
            return Random.Range(range.x, range.y);
        }
    }
}