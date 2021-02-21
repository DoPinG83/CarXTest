using Core.Common.Modules;
using Core.Gui;
using Core.Game.Managers;
using Core.UI.Tutorial;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Gui
{
    public class WinScreen : GuiController
    {
        [SerializeField] private Text rewardText;
        [SerializeField] private SpecialButton continueBtn;
        [SerializeField] private CanvasGroup continueBtnCanvasGroup;

        private CGame _game;
        private CConfig _config;

        protected override async void Init()
        {
            base.Init();

            _game = await App.Common.ResolveAsync<CGame>();
            _config = await App.Common.ResolveAsync<CConfig>();
            var core = _config.Core;

            continueBtn.enabled = false;
            continueBtnCanvasGroup.alpha = 0;
            LeanTween.alphaCanvas(continueBtnCanvasGroup, 1f, 0.3f)
                .setDelay(1.5f)
                .setIgnoreTimeScale(true).
                setOnComplete(_ =>
                {
                    continueBtn.enabled = true;
                    continueBtn.OnClickAsObservable().First().Subscribe(__ =>
                    {
                        _game.LoadNextLevel.Execute();
                    }).AddTo(ButtonBindings);
                });

            rewardText.text = core.levelRewards[Mathf.Min(core.levelRewards.Count - 1, _game.LvlNum.Value)].ToString();
        }
    }
}