namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;
    using UnityEngine.UI;

    public static class IObservableExtensions {
        public static IDisposable SubscribeScenario<T>(this IObservable<T> source, Scenario<T> scenario) {
            return source.Subscribe(scenario.Run);
        }

        public static IObservable<Unit> OnThrottledClickAsObservable(this Button button) {
            return button.onClick.AsObservable().ThrottleFirst(TimeSpan.FromSeconds(.5f));
        }
    }
}