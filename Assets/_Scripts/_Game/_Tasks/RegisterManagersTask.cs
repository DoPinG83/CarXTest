using System;
using Core.Common.Modules;
using Core.Common.Modules.Input;
using Core.Gui;
using Core.Game.Managers;
using UniRx;

namespace Core.Game.Initialization
{
    public class RegisterManagersTask : ILoadingTask
    {
        public int Weight { get { return 1; } }
        public float Progress { get { return (float)progress / 5f; } }
        public string LoadingText { get { return "Registring managers..."; } }
        public bool SendLoadingEvent { get; private set; }
        public string ErrorMsg { get; private set; }

        public string ErrorCode
        {
            get { return "RMT"; }
        }

        public Action<ILoadingTask> Complete { get; set; }
        public Action<ILoadingTask> Error { get; set; }

        int progress;

        public void Start()
        {
            WaitForManagersRegistration();
        }

        async void WaitForManagersRegistration()
        {
            var input = await Modules.All.ResolveAsync<CInput>();
            progress++;
            var gui = await Modules.All.ResolveAsync<CGUI>();
            progress++;
            var config = await App.Common.ResolveAsync<CConfig>();
            progress++;
            var data = await App.Common.ResolveAsync<CData>();
            progress++;
            var game = await App.Common.ResolveAsync<CGame>();
            progress++;

            Complete?.Invoke(this);
        }

        public void Stop()
        {
        }
    }
}
