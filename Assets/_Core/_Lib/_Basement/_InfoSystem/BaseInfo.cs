namespace Core.Common.InfoSystems {
    using InfoImplementation;

    public class BaseInfo : BaseInfoCore { }

    public class BaseInfo<T> : BaseInfoCore<T> where T : class {

        protected sealed override void Awake() {
            base.Awake();
        }

        protected sealed override void Start() {
            base.Start();
        }

        protected sealed override void OnEnable() {
            base.OnEnable();
        }

        protected sealed override void OnDisable() {
            base.OnDisable();
        }

        protected sealed override void OnDestroy() {
            base.OnDestroy();
        }

    }
}