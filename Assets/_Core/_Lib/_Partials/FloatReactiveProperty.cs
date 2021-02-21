//namespace UniRx {
//    using System;
//
//    
//    [Serializable]
//    public partial class FloatReactiveProperty : ReactiveProperty<float>
//    {
//        public static bool operator >(FloatReactiveProperty c1, FloatReactiveProperty c2) {
//            return c1.Value > c2.Value;
//        }
//
//        public static bool operator <(FloatReactiveProperty c1, FloatReactiveProperty c2) {
//            return c1.Value < c2.Value;
//        }
//        
//        public static float operator +(FloatReactiveProperty c1, FloatReactiveProperty c2) {
//            return c1.Value + c2.Value;
//        }
//
//        public static float operator -(FloatReactiveProperty c1, FloatReactiveProperty c2) {
//            return c1.Value - c2.Value;
//        }
//        
//        public static float operator *(FloatReactiveProperty c1, FloatReactiveProperty c2) {
//            return c1.Value * c2.Value;
//        }
//        
//        public static float operator /(FloatReactiveProperty c1, FloatReactiveProperty c2) {
//            return c1.Value / c2.Value;
//        }
//                
//        public static float operator %(FloatReactiveProperty c1, FloatReactiveProperty c2) {
//            return c1.Value % c2.Value;
//        }
//        
//        public static bool operator >(float c1, FloatReactiveProperty c2) {
//            return c1 > c2.Value;
//        }
//
//        public static bool operator <(float c1, FloatReactiveProperty c2) {
//            return c1 < c2.Value;
//        }
//        
//        public static float operator +(float c1, FloatReactiveProperty c2) {
//            return c1 + c2.Value;
//        }
//
//        public static float operator -(float c1, FloatReactiveProperty c2) {
//            return c1 - c2.Value;
//        }
//        
//        public static float operator *(float c1, FloatReactiveProperty c2) {
//            return c1 * c2.Value;
//        }
//        
//        public static float operator /(float c1, FloatReactiveProperty c2) {
//            return c1 / c2.Value;
//        }
//                
//        public static float operator %(float c1, FloatReactiveProperty c2) {
//            return c1 % c2.Value;
//        }
//        
//        public static bool operator >(FloatReactiveProperty c1, float c2) {
//            return c1.Value > c2;
//        }
//
//        public static bool operator <(FloatReactiveProperty c1, float c2) {
//            return c1.Value < c2;
//        }
//        
//        public static float operator +(FloatReactiveProperty c1, float c2) {
//            return c1.Value + c2;
//        }
//
//        public static float operator -(FloatReactiveProperty c1, float c2) {
//            return c1.Value - c2;
//        }
//        
//        public static float operator *(FloatReactiveProperty c1, float c2) {
//            return c1.Value * c2;
//        }
//        
//        public static float operator /(FloatReactiveProperty c1, float c2) {
//            return c1.Value / c2;
//        }
//                
//        public static float operator %(FloatReactiveProperty c1, float c2) {
//            return c1.Value % c2;
//        }
//        
//        public FloatReactiveProperty()
//            : base()
//        {
//
//        }
//
//        public FloatReactiveProperty(float initialValue)
//            : base(initialValue)
//        {
//
//        }
//    }
//}
