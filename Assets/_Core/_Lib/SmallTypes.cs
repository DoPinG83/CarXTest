namespace Core.Common.Misc {
    using System;
    using UnityEngine;

    [Serializable]
    public class SerializableVector3 {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() {
            return new Vector3(x, y, z);
        }
    }
    
    [Serializable]
    public class SerializableVector2 {
        public float x;
        public float y;
        
        public Vector2 ToVector2() {
            return new Vector2(x, y);
        }
    }

}
