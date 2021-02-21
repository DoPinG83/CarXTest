namespace Core.Common.InfoSystems.InfoImplementation {
    using System.Collections.Generic;
    using System.Reflection;
    using SystemImplementation;
    using Extensions;
    using UniRx;
    using UniRx.Async;
    using UnityEngine;

    public class BaseInfoCore : MonoBehaviour {
        public CompositeDisposable LifetimeDisposables = new CompositeDisposable();
    }

    public class BaseInfoCore<T> : BaseInfoCore where T : class {
        private Infos             infos;
        private LinkedListNode<T> _nodeInContainer;

        protected BaseInfoCore() {
            CheckForIncorrectMethods();
        }

        protected virtual void Awake() {
            infos = Infos.Instance;
            OnCreate();
        }

        protected virtual void Start() { }

        protected virtual void OnEnable() {
            OnInfoEnable()
                .ToObservable()
                .Subscribe(_ => infos.AddInfo(this as T, ref _nodeInContainer))
                .AddTo(LifetimeDisposables);
        }

        protected virtual void OnDisable() {
            LifetimeDisposables.Clear();
            infos?.RemoveInfo(this as T, _nodeInContainer);
        }

        protected virtual void OnDestroy() { }

        protected virtual async UniTask OnInfoEnable() { }

        protected virtual void OnCreate() { }

        public static LinkedList<T> GetAllInfos() {
            return Infos.Instance.GetInfosOfType(typeof(T)) as LinkedList<T>;
        }

        private void CheckForIncorrectMethods() {
            MethodInfo[] declaredMethods =
                typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            declaredMethods.ForEach(method => {
                if (method.Name == "Start" ||
                    method.Name == "Awake" ||
                    method.Name == "OnEnable" ||
                    method.Name == "OnDisable" ||
                    method.Name == "OnDestroy" ||
                    method.Name == "Update" ||
                    method.Name == "FixedUpdate") {
                    Debug.LogError($"[BaseInfo]: Prohibited method declared! BaseInfo = {typeof(T)}; methodName = {method.Name}");
                }
            });

        }
    }
}