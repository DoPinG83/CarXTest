using Core.Common.Modules;
using Core.Common.Modules.Input;
using Core.Gui;
using Core.Gui.Canvases;
using Core.Game.Managers;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Gui
{
    public class GameScreen : GuiController
    {
        private CInput _input;
        private CGame _game;
        private CConfig _config;

        protected override async void Init()
        {
            base.Init();

            _game = await App.Common.ResolveAsync<CGame>();
            _config = await App.Common.ResolveAsync<CConfig>();
            _input = await Modules.All.ResolveAsync<CInput>();
        }
    }
}
