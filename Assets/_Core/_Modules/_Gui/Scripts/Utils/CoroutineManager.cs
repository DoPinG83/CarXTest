using System.Collections;
using UnityEngine;
using Core.Utils;

namespace Core.Utils
{
    public interface ICoroutineManager
    {
        Coroutine Run(IEnumerator routine);
        void Stop(Coroutine routine);
    }

    public class CoroutineManager : MonoBehaviour, ICoroutineManager
    {
        CoroutineManager() { }

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            var instance = new GameObject("CoroutineManager [DontDestroyOnLoad]");
            DontDestroyOnLoad(instance);
            _instance = instance.AddComponent<CoroutineManager>();
        }

        #region Singleton
        static ICoroutineManager _instance;
        public static ICoroutineManager Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        Coroutine ICoroutineManager.Run(IEnumerator routine) => StartCoroutine(routine);
        void ICoroutineManager.Stop(Coroutine routine) => StopCoroutine(routine);
    }    
}

namespace System.Collections
{
    public static class CoroutineExtention
    {
        /// <summary>
        /// Runs global coroutine; 
        /// Do not use this for coroutines, that has references to objects, that can be destroyed during coroutine;
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        [Obsolete("Beware to use this method, object can be destroyed during coroutine")]
        public static Coroutine Run(this IEnumerator routine)
        {
            InsureCoroutineReferenceExists();
            return _coroutineManager.Run(routine);
        }

        public static void Stop(this Coroutine routine)
        {
            InsureCoroutineReferenceExists();
            _coroutineManager.Stop(routine);
        }

        static void InsureCoroutineReferenceExists()
        {
            if (_initialized)
                return;

            _initialized = true;
            _coroutineManager = CoroutineManager.Instance;
        }

        private static ICoroutineManager _coroutineManager;
        private static bool _initialized;
    }
}