using Core.Common.Modules;
using Core.Gui;
using Core.Game.Managers;
using Core.UI.Tutorial;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Gui
{
    public class LobbyScreen : GuiController
    {
        [SerializeField] private SpecialButton playBtn;
        [SerializeField] private Text playBtnText;
        
        private CGame _game;
        private CGUI _gui;

        protected override async void Init()
        {
            base.Init();

            _game = await App.Common.ResolveAsync<CGame>();
            _gui = await Modules.All.ResolveAsync<CGUI>();

           playBtn.OnClickAsObservable().First().Subscribe(_ =>
            {
                _game.StartGameplay.Execute();
            }).AddTo(ButtonBindings);
        }
    }
}
