using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Common
{

    public abstract class UpdatableBehaviour : MonoBehaviour
    {
        protected void SubscribeUpdate()
        {
            UpdateProvider.Instance.SubscribeUpdate(OnProviderUpdate);
        }

        protected void SubscribeFixedUpdate()
        {
            UpdateProvider.Instance.SubscribeFixedUpdate(OnProviderFixedUpdate);
        }

        protected void SubscribeLateUpdate()
        {
            UpdateProvider.Instance.SubscribeLateUpdate(OnProviderLateUpdate);
        }

        protected abstract void OnProviderUpdate(float dt);
        protected abstract void OnProviderFixedUpdate();
        protected abstract void OnProviderLateUpdate();

        protected void UnsubscribeUpdate()
        {
            if (UpdateProvider.IsAlive)
                UpdateProvider.Instance.UnSubscribeUpdate(OnProviderUpdate);
        }

        protected void UnsubscribeFixedUpdate()
        {
            if (UpdateProvider.IsAlive)
                UpdateProvider.Instance.UnSubscribeFixedUpdate(OnProviderFixedUpdate);
        }

        protected void UnsubscribeLateUpdate()
        {
            if (UpdateProvider.IsAlive)
                UpdateProvider.Instance.UnSubscribeLateUpdate(OnProviderLateUpdate);
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeUpdate();
            UnsubscribeFixedUpdate();
            UnsubscribeLateUpdate();
        }
    }
}
