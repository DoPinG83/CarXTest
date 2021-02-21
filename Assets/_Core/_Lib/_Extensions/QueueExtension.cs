namespace Core.Common.Extensions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class QueueExtension {
        public static void ForEach<T>(this Queue<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        } 
    }
}