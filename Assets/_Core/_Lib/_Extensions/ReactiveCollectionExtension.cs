namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;

    public static class ReactiveCollectionExtensions {
        public static void ForEach<T>(this ReactiveCollection<T> source, Action<T> action) {
            for( int i = 0; i < source.Count; i++ ) action(source[i]);
        }
        
        public static void ForEachPair<T>(this ReactiveCollection<T> source, Action<T, T> action) {
            for (int i = 0; i < source.Count; i++) {
                for (int j = i + 1; j < source.Count; j++) {                    
                    action(source[i], source[j]);   
                }
            }
        }
    }
}