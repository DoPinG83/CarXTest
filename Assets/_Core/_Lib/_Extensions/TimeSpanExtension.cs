namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using UniRx;
    using UnityEngine;
    
    public static class TimeSpanExtension {
        public static AsyncSubject<long> GetAwaiter(this TimeSpan timeSpan) {
            return Observable.Timer(timeSpan).GetAwaiter();
        } 
    }
}