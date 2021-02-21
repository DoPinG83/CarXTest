namespace Core.Common.InfoSystems.Example {
    using System.Collections;
    using System.Collections.Generic;
    using Core.Common.InfoSystems;
    using UnityEngine;

    public class ExampleSystem1 : BaseSystem<ExampleInfo1>, ISystemWithUpdate {
        protected override void OnUpdate(float dt, LinkedList<ExampleInfo1> info0LinkedList) {
            Debug.Log($"\t [EMIS1] -> info1count={info0LinkedList.Count};");
        }
    }
}