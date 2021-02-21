using System.Collections;
using System.Collections.Generic;
using Core.Gui;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Core.Game.Managers;

namespace Core.Game.Gui
{
    public class TopPanel : GuiController
    {
        [SerializeField] private Text levelText;
        [SerializeField] private Text moneyText;

        private CData _data;

        protected override async void Init()
        {
            base.Init();

            _data = await App.Common.ResolveAsync<CData>();

            _data.CurrentLevelNumber.Subscribe(value =>
            {
                levelText.text = string.Format("Level {0}", value + 1);
            })
            .AddTo(LifetimeDisposables);

            _data.Money.Subscribe(value =>
            {
                moneyText.text = value.ToString();
            })
            .AddTo(LifetimeDisposables);
        }
    }
}
