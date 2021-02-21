using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Game.Initialization
{
    public class SceneLoadingTask : ILoadingTask
    {
        public int Weight { get { return 1; } }
        public float Progress { get { return _asyncLoader == null ? 0 : _asyncLoader.progress; } }
        public string LoadingText { get { return "Loading scene..."; } }
        public bool SendLoadingEvent { get; private set; }
        public string ErrorMsg { get; private set; }

        public string ErrorCode
        {
            get { return "SLT"; }
        }

        public Action<ILoadingTask> Complete { get; set; }
        public Action<ILoadingTask> Error { get; set; }

        private AsyncOperation _asyncLoader;
        private string _loadingSceneName;

        private readonly CompositeDisposable _lifetimeDisposables = new CompositeDisposable();

        public SceneLoadingTask(string loadingSceneName)
        {
            _loadingSceneName = loadingSceneName;
        }

        public void Start()
        {
            var loader = Observable.FromMicroCoroutine(SceneLoader).Subscribe().AddTo(_lifetimeDisposables);
        }

        private IEnumerator SceneLoader()
        {
            _asyncLoader = SceneManager.LoadSceneAsync(_loadingSceneName, LoadSceneMode.Additive);
            _asyncLoader.allowSceneActivation = false;
            _asyncLoader.completed += OnSceneLoaded;

            while (_asyncLoader.isDone == false)
            {
                if (_asyncLoader.progress == 0.9f)
                {
                    _asyncLoader.allowSceneActivation = true;
                }
                yield return null;
            }
        }

        void OnSceneLoaded(AsyncOperation op)
        {
            _asyncLoader.completed -= OnSceneLoaded;
            if (op.isDone)
            {
                Complete?.Invoke(this);
            }
            else
            {
                ErrorMsg = string.Format("Can't load scene: {0}", _loadingSceneName);
                Error?.Invoke(this);
            }
            _lifetimeDisposables.Clear();
        }

        public void Stop()
        {
            _lifetimeDisposables.Clear();
        }
    }
}