namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;

    public static class BoolExtension {

        public static bool RandomBool(float probability = 0.5f) {
            probability = Mathf.Clamp(probability, 0, 1);
            return UnityEngine.Random.Range(0f, 1f) <= probability;
        }
        
    }
}