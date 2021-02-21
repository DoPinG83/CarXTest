namespace Core.Game.Infos {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx.Async;
    using Core.Common.InfoSystems;

    public class ColorSchemeInfo : BaseInfo<ColorSchemeInfo> {
    
        protected override async UniTask OnInfoEnable() { }

        public virtual void SetColorScheme(int schemeId)
        {
            
        }
    }
}