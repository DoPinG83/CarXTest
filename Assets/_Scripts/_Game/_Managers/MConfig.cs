using System.Collections.Generic;

namespace Core.Game.Managers 
{
    using Core.Common;
    using Core.Common.Extensions;
    using Configs;
    using UniRx;
    using UnityEngine;

    public class CConfig : Contract<CConfig> 
    {
        [Output] public readonly ReactiveProperty<CoreConfig> CoreConfig = new ReactiveProperty<CoreConfig>();
        public CoreConfig Core => CoreConfig.Value;
    }
    
    public class MConfig : MonoBehaviour 
    {
        [SerializeField] private CoreConfig coreConfig;

        private CConfig _contract;
        private readonly CompositeDisposable _lifetimeDisposables = new CompositeDisposable();
        
        
        private void OnEnable() 
        {
            _contract = this.Get<CConfig>();
            _contract.CoreConfig.Value = coreConfig;
            App.Common.Register(_contract).AddTo(_lifetimeDisposables);
        }

        
        private void OnDisable() 
        {
            _lifetimeDisposables.Clear();
        }
    }
}
