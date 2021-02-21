namespace Core.Common.InfoSystems.Example {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Core.Common.InfoSystems;
    using Extensions;
    using UniRx.Async;
    using UnityEngine;

    public class ExampleSystem : BaseSystem<ExampleInfo1, ExampleInfo2>, 
        ISystemWithUpdate, ISystemWithFixedUpdate {

        protected override async UniTask OnSystemEnable() {
            Debug.Log($"[ExampleSystem]: ExampleSystem started enabling.");
            await TimeSpan.FromSeconds(3f);
            Debug.Log($"[ExampleSystem]: ExampleSystem is enabled.");
        }

        protected override void OnSystemDisable() {
            Debug.Log($"[ExampleSystem]: ExampleSystem disabled.");
        }

        protected override void OnInfoRegistered(ExampleInfo1 info) {
            Debug.Log($"[ExampleSystem]: new info1 with IntProperty={info.IntProperty} registered. ({info.gameObject.name})");
        }

        protected override void OnInfoRegistered(ExampleInfo2 info) {
            Debug.Log($"[ExampleSystem]: new info2 with FloatProperty={info.FloatProperty} registered.");
        }

        protected override void OnInfoRemoved(ExampleInfo1 info) {
            Debug.Log($"[ExampleSystem]: info1 with IntProperty={info.IntProperty} was removed.");
        }

        protected override void OnInfoRemoved(ExampleInfo2 info) {
            Debug.Log($"[ExampleSystem]: info2 with FloatProperty={info.FloatProperty} was removed.");
        }

        protected override void OnUpdate(float dt, LinkedList<ExampleInfo1> info0LinkedList,
                                         LinkedList<ExampleInfo2> info1LinkedList) {
            Debug.Log(
                $"[ExampleSystem]: on update actions on {info0LinkedList.Count} of infos1 and {info1LinkedList.Count} of infos2.");
        }

        protected override void OnFixedUpdate(LinkedList<ExampleInfo1> info0LinkedList,
                                              LinkedList<ExampleInfo2> info1LinkedList) {
            Debug.Log(
                $"[ExampleSystem]: on fixed update actions on {info0LinkedList.Count} of infos1 and {info1LinkedList.Count} of infos2.");
        }

    }
}