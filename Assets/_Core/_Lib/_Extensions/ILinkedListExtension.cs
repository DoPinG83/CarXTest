namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;

    public static class ILinkedListExtension {

        public static void ForEach<T>(this LinkedList<T> source, Action<T> action) {
            foreach (T item in source) action(item);
        }

        /// <summary>
        /// Not recommended to use!
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RandomElement<T>(this LinkedList<T> source) {
            if (source.Count == 0) {
                Debug.LogWarning("Trying to get random from empty collection!");
                return default(T);
            }
            int index = UnityEngine.Random.Range(0, source.Count);
            int i = 0;
            foreach (T item in source)
                if (i++ == index)
                    return item;
            Debug.LogWarning("Couldn't get random element.'");
            return default(T);
        }

        /// <summary>
        /// Not recommended to use!
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetRandomAndRemove<T>(this LinkedList<T> source) {
            if (source.Count == 0) {
                Debug.LogWarning("Trying to get random from empty collection!");
                return default(T);
            }

            int index = UnityEngine.Random.Range(0, source.Count);
            int i = 0;
            foreach (T item in source) {
                if (i++ == index) {
                    var outer = item;
                    source.Remove(outer);
                    return outer;
                }
            }
            Debug.LogWarning("Couldn't get random element.'");
            return default(T);
        }

    }
}