namespace Core.Common.InfoSystems.SystemImplementation {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;
    using System.Linq;
    using System.Reflection;
    using Core.Common.Extensions;
    using InfoImplementation;
    using UniRx.Async;


    public class BaseSystemCore : UpdatableBehaviour
    {

        protected CompositeDisposable    LifetimeDisposables = new CompositeDisposable();
        private   ReactiveProperty<bool> _initialized        = new ReactiveProperty<bool>();

        private bool _withUpdate;
        private bool _withFixedUpdate;
        private bool _withLateUpdate;

        protected ReactiveProperty<bool> Initialized     => _initialized;

        protected BaseSystemCore() {
            CheckForIncorrectMethods(GetType());
        }

        protected virtual void Awake() {
            _withUpdate = this.GetType().GetInterfaces().Contains(typeof(ISystemWithUpdate));
            if (_withUpdate)
                SubscribeUpdate();

            _withFixedUpdate = this.GetType().GetInterfaces().Contains(typeof(ISystemWithFixedUpdate));
            if(_withFixedUpdate)
                SubscribeFixedUpdate();

            _withLateUpdate = this.GetType().GetInterfaces().Contains(typeof(ISystemWithLateUpdate));
            if (_withLateUpdate)
                SubscribeLateUpdate();

            OnCreate();
        }

        protected virtual void OnEnable() {
            _initialized.Value = false;
            OnSystemEnableWithErrorHandling();
        }

        private void OnDisable() {
            OnSystemDisable();
            LifetimeDisposables.Clear();
        }

        protected override void OnProviderUpdate(float dt) {
            if (!Initialized.Value) return;
            OnUpdate(dt);
        }

        protected override void OnProviderFixedUpdate() {
            if (!Initialized.Value) return;
            OnFixedUpdate();
        }

        protected override void OnProviderLateUpdate()
        {
            if (!Initialized.Value) return;
            OnLateUpdate();
        }

        private async void OnSystemEnableWithErrorHandling() {
            await OnSystemEnable();
            _initialized.Value = true;
        }

        protected virtual async UniTask OnSystemEnable() { }
        protected virtual void OnCreate() { }
        protected virtual void OnSystemDisable() { }

        protected virtual void OnUpdate(float dt) { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate() { }

        private void CheckForIncorrectMethods(Type systemType) {
            MethodInfo[] declaredMethods =
                systemType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                      BindingFlags.DeclaredOnly);
            declaredMethods.ForEach(method => {
                if (method.Name == "Start" ||
                    method.Name == "Awake" ||
                    method.Name == "OnEnable" ||
                    method.Name == "OnDisable" ||
                    method.Name == "OnDestroy" ||
                    method.Name == "Update" ||
                    method.Name == "FixedUpdate") {
                    Debug.LogError(
                        $"[BaseSystem]: Prohibited method declared! BaseSystem = {systemType}; methodName = {method.Name}");
                }
            });
        }

    }

    public class BaseSystemCore<T0> : BaseSystemCore where T0 : MonoBehaviour {

        private   Infos _infosContainer;
        private   Type  _T0Type;
        protected Infos InfosContainer => _infosContainer;
        protected Type  T0Type         => _T0Type;

        protected LinkedList<T0> Info0List =>
            InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>;

        protected override void Awake() {
            base.Awake();
            _infosContainer = Infos.Instance;
            _T0Type = typeof(T0);
        }

        protected override void OnEnable() {
            base.OnEnable();
            var registeredInfos = InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>;
            registeredInfos?.ForEach(OnNewInfoRegistered);
            InfosContainer.NewInfoAdded.Subscribe(OnNewInfoRegistered).AddTo(LifetimeDisposables);
            InfosContainer.InfoRemoved.Subscribe(infoData => {
                if (!Initialized.Value) return;
                OnInfoRemoved(infoData);
            }).AddTo(LifetimeDisposables);
        }

        protected override void OnProviderUpdate(float dt) {
            if (!Initialized.Value) return;
            OnUpdate(dt, InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>);
        }

        protected override void OnProviderFixedUpdate() {
            if (!Initialized.Value) return;
            OnFixedUpdate(InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>);
        }

        private void OnNewInfoRegistered((Type, object) infoData) {
            if (Initialized.Value) {
                OnNewInfoAdded(infoData);
                return;
            }
            BaseInfoCore bInfo = infoData.Item2 as BaseInfoCore;
            Initialized
                .Where(x => x)
                .First()
                .Subscribe(_ => OnNewInfoAdded(infoData))
                .AddTo(bInfo.LifetimeDisposables);
        }

        private void OnNewInfoRegistered(T0 info) {
            if (!info.gameObject.activeInHierarchy) return;
            if (Initialized.Value) {
                OnInfoRegistered(info);
                return;
            }
            BaseInfoCore bInfo = info as BaseInfoCore;
            Initialized
                .Where(x => x)
                .First()
                .Subscribe(_ => {
                    if (!info.gameObject.activeInHierarchy) return;
                    OnInfoRegistered(info);
                }).AddTo(bInfo.LifetimeDisposables);
        }

        protected virtual async void OnNewInfoAdded((Type, object) infoData) {
            (Type item1, object item2) = infoData;
            if (item1 != T0Type) return;
            T0 info = (T0) item2;
            if (info.gameObject.activeInHierarchy) OnInfoRegistered(info);
        }

        protected virtual void OnInfoRemoved((Type, object) infoData) {
            (Type item1, object item2) = infoData;
            if (item1 == T0Type) OnInfoRemoved((T0) item2);
        }

        protected virtual void OnInfoRegistered(T0 info) { }
        protected virtual void OnInfoRemoved(T0 info) { }

        protected virtual void OnUpdate(float dt, LinkedList<T0> info0LinkedList) { }

        protected virtual void OnFixedUpdate(LinkedList<T0> info0LinkedList) { }

        protected sealed override void OnUpdate(float dt) { }
        protected sealed override void OnFixedUpdate() { }

    }

    public class BaseSystemCore<T0, T1> : BaseSystemCore<T0> where T0 : MonoBehaviour
                                                             where T1 : MonoBehaviour {

        private   Type           _T1Type;
        protected Type           T1Type    => _T1Type;
        protected LinkedList<T1> Info1List => InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>;

        protected override void Awake() {
            base.Awake();
            _T1Type = typeof(T1);
        }

        protected override void OnEnable() {
            base.OnEnable();
            var registeredInfos = InfosContainer.GetInfosOfType(_T1Type) as LinkedList<T1>;
            registeredInfos?.ForEach(OnNewInfoRegistered);
        }

        protected override void OnProviderUpdate(float dt)
        {
            if (!Initialized.Value) return;
            OnUpdate(dt,
                InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>,
                InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>);
        }

        protected override void OnProviderFixedUpdate() {
            if (!Initialized.Value) return;
            OnFixedUpdate(InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>,
                InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>);
        }

        private void OnNewInfoRegistered(T1 info) {
            if (!info.gameObject.activeInHierarchy) return;
            if (Initialized.Value) {
                OnInfoRegistered(info);
                return;
            }
            BaseInfoCore bInfo = info as BaseInfoCore;
            Initialized
                .Where(x => x)
                .First()
                .Subscribe(_ => {
                    if (!info.gameObject.activeInHierarchy) return;
                    OnInfoRegistered(info);
                }).AddTo(bInfo.LifetimeDisposables);
        }

        protected override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
            (Type item1, object item2) = infoData;
            if (item1 != T1Type) return;
            T1 info = (T1) item2;
            if (info.gameObject.activeInHierarchy) OnInfoRegistered(info);
        }

        protected override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
            (Type item1, object item2) = infoData;
            if (item1 == T1Type) OnInfoRemoved((T1) item2);
        }

        protected virtual void OnInfoRegistered(T1 info) { }
        protected virtual void OnInfoRemoved(T1 info) { }
        protected virtual void OnUpdate(float dt, LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList) { }
        protected virtual void OnFixedUpdate(LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList) { }

        protected sealed override void OnUpdate(float dt, LinkedList<T0> info0LinkedList) { }
        protected sealed override void OnFixedUpdate(LinkedList<T0> info0LinkedList) { }

    }


    public class BaseSystemCore<T0, T1, T2> : BaseSystemCore<T0, T1> where T0 : MonoBehaviour
                                                                     where T1 : MonoBehaviour
                                                                     where T2 : MonoBehaviour {

        private   Type           _T2Type;
        protected Type           T2Type    => _T2Type;
        protected LinkedList<T2> Info2List => InfosContainer.GetInfosOfType(T2Type) as LinkedList<T2>;

        protected override void Awake() {
            base.Awake();
            _T2Type = typeof(T2);
        }

        protected override void OnEnable() {
            base.OnEnable();
            var registeredInfos = InfosContainer.GetInfosOfType(_T2Type) as LinkedList<T2>;
            registeredInfos?.ForEach(OnNewInfoRegistered);
        }

        protected override void OnProviderUpdate(float dt) {
            if (!Initialized.Value) return;
            OnUpdate(dt,
                InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>,
                InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>,
                InfosContainer.GetInfosOfType(T2Type) as LinkedList<T2>);
        }

        protected override void OnProviderFixedUpdate() {
            if (!Initialized.Value) return;
            OnFixedUpdate(InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>,
                InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>,
                InfosContainer.GetInfosOfType(T2Type) as LinkedList<T2>);
        }

        private void OnNewInfoRegistered(T2 info) {
            if (!info.gameObject.activeInHierarchy) return;
            if (Initialized.Value) {
                OnInfoRegistered(info);
                return;
            }
            BaseInfoCore bInfo = info as BaseInfoCore;
            Initialized
                .Where(x => x)
                .First()
                .Subscribe(_ => {
                    if (!info.gameObject.activeInHierarchy) return;
                    OnInfoRegistered(info);
                }).AddTo(bInfo.LifetimeDisposables);
        }

        protected override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
            (Type item1, object item2) = infoData;
            if (item1 != T2Type) return;
            T2 info = (T2) item2;
            if (info.gameObject.activeInHierarchy) OnInfoRegistered(info);
        }

        protected override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
            (Type item1, object item2) = infoData;
            if (item1 == T2Type) OnInfoRemoved((T2) item2);
        }

        protected virtual void OnInfoRegistered(T2 info) { }
        protected virtual void OnInfoRemoved(T2 info) { }

        protected virtual void OnUpdate(float dt, LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                        LinkedList<T2> info2LinkedList) { }

        protected virtual void OnFixedUpdate(LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                             LinkedList<T2> info2LinkedList) { }

        protected sealed override void OnUpdate(float dt, LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList) { }
        protected sealed override void OnFixedUpdate(LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList) { }

    }


    public class BaseSystemCore<T0, T1, T2, T3> : BaseSystemCore<T0, T1, T2> where T0 : MonoBehaviour
                                                                             where T1 : MonoBehaviour
                                                                             where T2 : MonoBehaviour
                                                                             where T3 : MonoBehaviour {

        private   Type           _T3Type;
        protected Type           T3Type    => _T3Type;
        protected LinkedList<T3> Info3List => InfosContainer.GetInfosOfType(T3Type) as LinkedList<T3>;

        protected override void Awake() {
            base.Awake();
            _T3Type = typeof(T3);
        }

        protected override void OnEnable() {
            base.OnEnable();
            var registeredInfos = InfosContainer.GetInfosOfType(_T3Type) as LinkedList<T3>;
            registeredInfos?.ForEach(OnNewInfoRegistered);
        }

        protected override void OnProviderUpdate(float dt) {
            if (!Initialized.Value) return;
            OnUpdate(dt,
                InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>,
                InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>,
                InfosContainer.GetInfosOfType(T2Type) as LinkedList<T2>,
                InfosContainer.GetInfosOfType(T3Type) as LinkedList<T3>);
        }

        protected override void OnProviderFixedUpdate() {
            if (!Initialized.Value) return;
            OnFixedUpdate(InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>,
                InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>,
                InfosContainer.GetInfosOfType(T2Type) as LinkedList<T2>,
                InfosContainer.GetInfosOfType(T3Type) as LinkedList<T3>);
        }

        private void OnNewInfoRegistered(T3 info) {
            if (!info.gameObject.activeInHierarchy) return;
            if (Initialized.Value) {
                OnInfoRegistered(info);
                return;
            }
            BaseInfoCore bInfo = info as BaseInfoCore;
            Initialized
                .Where(x => x)
                .First()
                .Subscribe(_ => {
                    if (!info.gameObject.activeInHierarchy) return;
                    OnInfoRegistered(info);
                }).AddTo(bInfo.LifetimeDisposables);
        }

        protected override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
            (Type item1, object item2) = infoData;
            if (item1 != T3Type) return;
            T3 info = (T3) item2;
            if (info.gameObject.activeInHierarchy) OnInfoRegistered(info);
        }

        protected override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
            (Type item1, object item2) = infoData;
            if (item1 == T3Type) OnInfoRemoved((T3) item2);
        }

        protected virtual void OnInfoRegistered(T3 info) { }
        protected virtual void OnInfoRemoved(T3 info) { }

        protected virtual void OnUpdate(float dt, LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                        LinkedList<T2> info2LinkedList, LinkedList<T3> info3LinkedList) { }

        protected virtual void OnFixedUpdate(LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                             LinkedList<T2> info2LinkedList, LinkedList<T3> info3LinkedList) { }

        protected sealed override void OnUpdate(float dt, LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                                LinkedList<T2> info2LinkedList) { }

        protected sealed override void OnFixedUpdate(LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                                     LinkedList<T2> info2LinkedList) { }

    }

    public class BaseSystemCore<T0, T1, T2, T3, T4> : BaseSystemCore<T0, T1, T2, T3> where T0 : MonoBehaviour
                                                                                     where T1 : MonoBehaviour
                                                                                     where T2 : MonoBehaviour
                                                                                     where T3 : MonoBehaviour
                                                                                     where T4 : MonoBehaviour {

        private   Type           _T4Type;
        protected Type           T4Type    => _T4Type;
        protected LinkedList<T4> Info4List => InfosContainer.GetInfosOfType(T4Type) as LinkedList<T4>;

        protected override void Awake() {
            base.Awake();
            _T4Type = typeof(T4);
        }

        protected override void OnEnable() {
            base.OnEnable();
            var registeredInfos = InfosContainer.GetInfosOfType(_T4Type) as LinkedList<T4>;
            registeredInfos?.ForEach(OnNewInfoRegistered);
        }

        protected override void OnProviderUpdate(float dt) {
            if (!Initialized.Value) return;
            OnUpdate(dt,
                InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>,
                InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>,
                InfosContainer.GetInfosOfType(T2Type) as LinkedList<T2>,
                InfosContainer.GetInfosOfType(T3Type) as LinkedList<T3>,
                InfosContainer.GetInfosOfType(T4Type) as LinkedList<T4>);
        }

        protected override void OnProviderFixedUpdate() {
            if (!Initialized.Value) return;
            OnFixedUpdate(InfosContainer.GetInfosOfType(T0Type) as LinkedList<T0>,
                InfosContainer.GetInfosOfType(T1Type) as LinkedList<T1>,
                InfosContainer.GetInfosOfType(T2Type) as LinkedList<T2>,
                InfosContainer.GetInfosOfType(T3Type) as LinkedList<T3>,
                InfosContainer.GetInfosOfType(T4Type) as LinkedList<T4>);
        }

        private void OnNewInfoRegistered(T4 info) {
            if (!info.gameObject.activeInHierarchy) return;
            if (Initialized.Value) {
                OnInfoRegistered(info);
                return;
            }
            BaseInfoCore bInfo = info as BaseInfoCore;
            Initialized
                .Where(x => x)
                .First()
                .Subscribe(_ => {
                    if (!info.gameObject.activeInHierarchy) return;
                    OnInfoRegistered(info);
                }).AddTo(bInfo.LifetimeDisposables);
        }

        protected override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
            (Type item1, object item2) = infoData;
            if (item1 != T4Type) return;
            T4 info = (T4) item2;
            if (info.gameObject.activeInHierarchy) OnInfoRegistered(info);
        }

        protected override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
            (Type item1, object item2) = infoData;
            if (item1 == T4Type) OnInfoRemoved((T4) item2);
        }

        protected virtual void OnInfoRegistered(T4 info) { }
        protected virtual void OnInfoRemoved(T4 info) { }

        protected virtual void OnUpdate(float dt, LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                        LinkedList<T2> info2LinkedList, LinkedList<T3> info3LinkedList,
                                        LinkedList<T4> info4LinkedList) { }

        protected virtual void OnFixedUpdate(LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                             LinkedList<T2> info2LinkedList, LinkedList<T3> info3LinkedList,
                                             LinkedList<T4> info4LinkedList) { }

        protected sealed override void OnUpdate(float dt, LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                                LinkedList<T2> info2LinkedList, LinkedList<T3> info3LinkedList) { }

        protected sealed override void OnFixedUpdate(LinkedList<T0> info0LinkedList, LinkedList<T1> info1LinkedList,
                                                     LinkedList<T2> info2LinkedList, LinkedList<T3> info3LinkedList) { }

    }

}