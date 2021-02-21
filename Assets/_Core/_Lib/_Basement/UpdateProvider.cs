using System;
using System.Linq;
using Core.Utils;
using UnityEngine;

namespace Core.Common
{
    public class UpdateProvider : MonoBehaviour
    {
        private event Action<float> OnUpdate;
        private event Action OnFixedUpdate;
        private event Action OnLateUpdate;

        public void SubscribeUpdate(Action<float> update)
        {
            if (OnUpdate != null)
            {
                Delegate[] invocationList = OnUpdate.GetInvocationList();
                if (invocationList.Contains(update))
                    return;
            }
            OnUpdate += update;
        }

        public void UnSubscribeUpdate(Action<float> update)
        {
            OnUpdate -= update;
        }

        public void SubscribeFixedUpdate(Action fixedUpdate)
        {
            if (OnFixedUpdate != null)
            {
                Delegate[] invocationList = OnFixedUpdate.GetInvocationList();
                if (invocationList.Contains(fixedUpdate))
                    return;
            }
            OnFixedUpdate += fixedUpdate;
        }

        public void UnSubscribeFixedUpdate(Action fixedUpdate)
        {
            OnFixedUpdate -= fixedUpdate;
        }

        public void SubscribeLateUpdate(Action lateUpdate)
        {
            if (OnLateUpdate != null)
            {
                Delegate[] invocationList = OnLateUpdate.GetInvocationList();
                if (invocationList.Contains(lateUpdate))
                    return;
            }
            OnLateUpdate += lateUpdate;
        }

        public void UnSubscribeLateUpdate(Action lateUpdate)
        {
            OnLateUpdate -= lateUpdate;
        }

        #region singleton
        private bool _alive;
        private static volatile UpdateProvider _curMgr;

        private UpdateProvider()
        {
        }

        public static UpdateProvider Instance
        {
            get
            {
                if (_curMgr == null)
                {
                    var inScene = FindObjectOfType<UpdateProvider>();
                    if (inScene != null)
                    {
                        CoreLog.LogError("Dont create UpdateProvider manually");
                        _curMgr = inScene;
                    }
                    else
                        _curMgr = new GameObject(typeof(UpdateProvider).Name)
                            .AddComponent<UpdateProvider>();

                    DontDestroyOnLoad(_curMgr.gameObject);
                    _curMgr._alive = true;
                }
                return _curMgr;
            }
        }

        public static bool IsAlive
        {
            get
            {
                if (_curMgr == null)
                    return false;

                return _curMgr._alive;
            }
        }

        private void OnDestroy()
        {
            _alive = false;
        }

        private void OnApplicationQuit()
        {
            _alive = false;
        }
        #endregion

        private void Update()
        {
            float dt = Time.deltaTime;
            if (OnUpdate != null)
                OnUpdate(dt);
        }

        private void FixedUpdate()
        {
            if (OnFixedUpdate != null)
                OnFixedUpdate();
        }

        private void LateUpdate()
        {
            if (OnLateUpdate != null)
                OnLateUpdate();
        }
    }
}
