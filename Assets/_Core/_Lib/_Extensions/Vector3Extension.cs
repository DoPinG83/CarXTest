namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;
    
    public static class Vector3Extension {
        public static Vector2 GetXZ(this Vector3 source) {
            return new Vector2(source.x, source.z);
        }

        public static Vector3 GetX0Z(this Vector3 source) {
            return new Vector3(source.x, 0, source.z);
        }
        
        public static Vector3 GetXY0(this Vector3 source) {
            return new Vector3(source.x, source.y, 0);
        }
        
        public static bool ApproxEq(this Vector3 a, Vector3 b, float sigma = 0.001f) {
            return Mathf.Abs(a.x - b.x) <= sigma
                   && Mathf.Abs(a.y - b.y) <= sigma
                   && Mathf.Abs(a.z - b.z) <= sigma;
        }
    }
}