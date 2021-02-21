namespace Core.Common.InfoSystems.Example {
    using System.Collections;
    using System.Collections.Generic;
    using Core.Common.InfoSystems;
    using UnityEngine;

    public class ExampleSystem2 : BaseSystem<ExampleInfo1, ExampleInfo2, ExampleInfo3, ExampleInfo4, ExampleInfo5>,
        ISystemWithUpdate {

        protected override void OnInfoRegistered(ExampleInfo3 info) {
            Debug.Log($"\t [EMIS] -> new info2 #{info.gameObject.name}# -> {info.floatProperty}");
        }

        protected override void OnInfoRegistered(ExampleInfo5 info) {
            Debug.Log($"\t [EMIS] -> new info1 #{info.gameObject.name}# -> {info.intProperty}");
        }

        protected override void OnUpdate(float dt,
                                         LinkedList<ExampleInfo1> info0LinkedList,
                                         LinkedList<ExampleInfo2> info1LinkedList,
                                         LinkedList<ExampleInfo3> info2LinkedList,
                                         LinkedList<ExampleInfo4> info3LinkedList,
                                         LinkedList<ExampleInfo5> info4LinkedList) {
            Debug.Log(
                $"\t [EMIS] -> info1count={info0LinkedList.Count}; info2count={info1LinkedList.Count}; info3count={info2LinkedList.Count}; info4count={info3LinkedList.Count}; info5count={info4LinkedList.Count};");
        }

    }
}