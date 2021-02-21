namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;
    
    public static class IListExtension {
        public static void For<T>(this IList<T> source, Action<T, int> action) {
            for( int i = 0; i < source.Count; i++ ) action(source[i], i);
        }

        public static void ForEach<T>(this IList<T> source, Action<T> action) {
            for( int i = 0; i < source.Count; i++ ) action(source[i]);
        }

        public static T RandomElement<T>(this IList<T> source) {
            int index = UnityEngine.Random.Range(0, source.Count);
            return source[index];
        }

        public static T GetRandomAndRemove<T>(this IList<T> source) {
            if (source.Count == 0) {
                Debug.LogWarning("Trying to get random from empty collection!");
                return default(T);
            }
            
            int index = UnityEngine.Random.Range(0, source.Count);
            var outer = source[index];
            
            source.RemoveAt(index);
            
            return outer;
        }
        
        public static T GetFirst<T>(this IList<T> source) {
            return source[0];
        }

        public static T GetLast<T>(this IList<T> source) {
            return source[source.Count - 1];
        }
        
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public static void Shuffle<T>(this IList<T> ts) {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }
}