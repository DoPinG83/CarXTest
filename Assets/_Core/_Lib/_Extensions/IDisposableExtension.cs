namespace Core.Common.Extensions {
    using System;
    using Common;
    using UniRx;
    using UnityEngine;
    
    public static class IDisposableExtension {
        public static T AddTo<T>(this T disposable, Contract contract) where T : IDisposable {
            if (!(contract?.Target is GameObject)) {
                disposable.Dispose();
                return disposable;
            }

            disposable.AddTo((GameObject) contract.Target);
            return disposable;
        }
    }
}