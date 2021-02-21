using Newtonsoft.Json;

namespace Core.Game.Managers
{
    using System;
    using UnityEngine;
    using UniRx;
    using Core.Common;
    using Core.Common.Modules;
    using Core.Common.Extensions;

    public class CData : Contract<CData>
    {
        [Output] public readonly IntReactiveProperty Money = new IntReactiveProperty();
        [Output] public readonly IntReactiveProperty CurrentLevelNumber = new IntReactiveProperty();
        [Input] public readonly ReactiveCommand ResetData = new ReactiveCommand();
    }

    public class MData : MonoBehaviour
    {
        private CData _contract;
        private CGame _game;
        private CConfig _config;

        private readonly CompositeDisposable _lifetimeDisposables = new CompositeDisposable();

        
        private async void OnEnable()
        {
            _contract = this.Get<CData>();
            _config = await App.Common.ResolveAsync<CConfig>();
            
            LoadData();
            
            App.Common.Register(_contract).AddTo(_lifetimeDisposables);
            
            BindDataSaving();

            _contract.ResetData.Subscribe(_ => PlayerPrefs.DeleteAll()).AddTo(_lifetimeDisposables);
        }

        
        private void LoadData()
        {
            _contract.CurrentLevelNumber.Value = PlayerPrefs.GetInt(PrefsNames.CURRENT_LEVEL);
            _contract.Money.Value = PlayerPrefs.GetInt(PrefsNames.MONEY);
        }


        private async void BindDataSaving()
        {
            _contract.Money.Subscribe(val => PlayerPrefs.SetInt(PrefsNames.MONEY, val)).AddTo(_lifetimeDisposables);
            _contract.CurrentLevelNumber.Subscribe(val => PlayerPrefs.SetInt(PrefsNames.CURRENT_LEVEL, val)).AddTo(_lifetimeDisposables);
        }
        
        
        private bool GetBoolFromPrefs(string prefKey, bool noKeyResult = false)
        {
            if (!PlayerPrefs.HasKey(prefKey)) return noKeyResult;
            if (PlayerPrefs.GetInt(prefKey) == 0) return false;
            return true;
        }

        
        private void SetBoolToPrefs(string prefKey, bool setValue)
        {
            PlayerPrefs.SetInt(prefKey, setValue ? 1 : 0);
        }
  
        private void OnDisable()
        {
            _lifetimeDisposables.Clear();
        }
        
                
        private static class PrefsNames
        {
            public const string CURRENT_LEVEL = "CurrentLevelNumber";
            public const string MONEY = "Money";
        }
        
    }
}
