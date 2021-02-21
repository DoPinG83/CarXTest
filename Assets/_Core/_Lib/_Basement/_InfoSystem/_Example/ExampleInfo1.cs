namespace Core.Common.InfoSystems.Example {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Core.Common.InfoSystems;
    using Extensions;
    using UniRx.Async;
    using UnityEngine;

    public class ExampleInfo1 : BaseInfo<ExampleInfo1> {

        [SerializeField] private int _intProperty;

        protected override async UniTask OnInfoEnable() {
            Debug.Log($"[ExampleInfo1]: ExampleInfo1 started enabling.");
            await TimeSpan.FromSeconds(2f);
            Debug.Log($"[ExampleInfo1]: ExampleInfo1 is enabled.");
        }

        public int IntProperty {
            get => _intProperty;
            set => _intProperty = value;
        }
    }
    
}