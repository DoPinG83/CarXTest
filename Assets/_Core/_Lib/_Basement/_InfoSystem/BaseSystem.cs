namespace Core.Common.InfoSystems {
    using System;
    using SystemImplementation;
    using UnityEngine;

    public interface ISystemWithUpdate { }

    public interface ISystemWithFixedUpdate { }

    public interface ISystemWithLateUpdate { }

    public class BaseSystem : BaseSystemCore {
        protected sealed override void Awake() {
            base.Awake();
        }

        protected sealed override void OnEnable() {
            base.OnEnable();
        }
    }

    public class BaseSystem<T0> : BaseSystemCore<T0> where T0 : MonoBehaviour {
        protected sealed override void Awake() {
            base.Awake();
        }

        protected sealed override void OnEnable() {
            base.OnEnable();
        }

        protected sealed override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
        }

        protected sealed override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
        }
    }

    public class BaseSystem<T0, T1> : BaseSystemCore<T0, T1> where T0 : MonoBehaviour
                                                             where T1 : MonoBehaviour {
        protected sealed override void Awake() {
            base.Awake();
        }

        protected sealed override void OnEnable() {
            base.OnEnable();
        }

        protected sealed override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
        }

        protected sealed override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
        }
    }

    public class BaseSystem<T0, T1, T2> : BaseSystemCore<T0, T1, T2>
        where T0 : MonoBehaviour where T1 : MonoBehaviour where T2 : MonoBehaviour {
        protected sealed override void Awake() {
            base.Awake();
        }

        protected sealed override void OnEnable() {
            base.OnEnable();
        }

        protected sealed override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
        }

        protected sealed override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
        }
    }

    public class BaseSystem<T0, T1, T2, T3> : BaseSystemCore<T0, T1, T2, T3> where T0 : MonoBehaviour
                                                                             where T1 : MonoBehaviour
                                                                             where T2 : MonoBehaviour
                                                                             where T3 : MonoBehaviour {
        protected sealed override void Awake() {
            base.Awake();
        }

        protected sealed override void OnEnable() {
            base.OnEnable();
        }

        protected sealed override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
        }

        protected sealed override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
        }
    }

    public class BaseSystem<T0, T1, T2, T3, T4> : BaseSystemCore<T0, T1, T2, T3, T4> where T0 : MonoBehaviour
                                                                                     where T1 : MonoBehaviour
                                                                                     where T2 : MonoBehaviour
                                                                                     where T3 : MonoBehaviour
                                                                                     where T4 : MonoBehaviour {
        protected sealed override void Awake() {
            base.Awake();
        }

        protected sealed override void OnEnable() {
            base.OnEnable();
        }

        protected sealed override void OnNewInfoAdded((Type, object) infoData) {
            base.OnNewInfoAdded(infoData);
        }

        protected sealed override void OnInfoRemoved((Type, object) infoData) {
            base.OnInfoRemoved(infoData);
        }
    }

}