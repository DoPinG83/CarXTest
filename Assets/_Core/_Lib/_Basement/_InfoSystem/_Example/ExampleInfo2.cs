namespace Core.Common.InfoSystems.Example {
    using System.Collections;
    using System.Collections.Generic;
    using Core.Common.InfoSystems;
    using UnityEngine;

    public class ExampleInfo2 : BaseInfo<ExampleInfo2> {

        [SerializeField] private float _floatProperty;

        public float FloatProperty {
            get => _floatProperty;
            set => _floatProperty = value;
        }
    }
}