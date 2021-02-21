namespace Core.Game.Managers
{
    using Core.Common;
    using Core.Common.Extensions;
    using Core.Common.Modules;
    using UniRx;
    using UnityEngine;
    using Core.Gui;
    using PathologicalGames;
    using Core.Utils;
    using Core.Game.Infos;
    using Core.Game.Gui;
    using System;
    using Core.Game.Configs;

    public class CGame : Contract<CGame>
    {
        [Output] public readonly ReactiveProperty<LevelInfo> CurrentLevel = new ReactiveProperty<LevelInfo>();
        [Output] public readonly IntReactiveProperty CurrentReward = new IntReactiveProperty();
        [Output] public readonly ReactiveProperty<int> LvlNum = new ReactiveProperty<int>();

        [Input] public readonly ReactiveCommand StartGameplay = new ReactiveCommand();
        [Input] public readonly ReactiveCommand LoadNextLevel = new ReactiveCommand();
        [Input] public readonly ReactiveCommand<bool> RestartLevel = new ReactiveCommand<bool>();
        [Input] public readonly ReactiveCommand<bool> OnLevelCompleted = new ReactiveCommand<bool>();
    }
    
    
    public class MGame : MonoBehaviour
    {
        [SerializeField] Camera mainCamera;
        private CGame _contract;
        private CConfig _config;
        private CData _data;
        private CGUI _guiManager;

        private SpawnPool _spawnPool;

        private readonly CompositeDisposable _lifetimeDisposables = new CompositeDisposable();

        public int lvlNum { get; private set; }

        bool _loadingQueueFinished;
        
        private async void OnEnable()
        {
            _contract = this.Get<CGame>();
            _config = await App.Common.ResolveAsync<CConfig>();
            _data = await App.Common.ResolveAsync<CData>();
            _guiManager = await Modules.All.ResolveAsync<CGUI>();

            _spawnPool = PoolManager.Pools["SpawnPool"];

            _contract.LoadNextLevel.Subscribe(_ => LoadNextLevel()).AddTo(_lifetimeDisposables);
            _contract.RestartLevel.Subscribe(LoadLevel).AddTo(_lifetimeDisposables);
            _contract.OnLevelCompleted.Subscribe(OnLevelCompleted).AddTo(_lifetimeDisposables);
            _contract.StartGameplay.Subscribe(_=>StartGameplay()).AddTo(_lifetimeDisposables);

            LoadLevel();

            App.Common.Register(_contract).AddTo(_lifetimeDisposables);

            Broadcaster.Instance.AddListener(BroadcasterEventCodes.LoadingQueueFinished, OnLoadingQueueFinished);
        }

        void OnLoadingQueueFinished(EventParams obj)
        {
            _loadingQueueFinished = true;
            _guiManager.GuiManager.Value.Show<LobbyScreen>(DisplayMode.Screen);
            _guiManager.GuiManager.Value.Show<TopPanel>(DisplayMode.Persistent);

            _guiManager.DoFadeOut.Execute((1f, null));
        }

        private void StartGameplay()
        {
            App.Systems.gameObject.SetActive(true);
            _guiManager.GuiManager.Value.Show<GameScreen>(DisplayMode.Screen);
        }


        private async void LoadLevel(bool ChangeLvlNum = true)
        {
            if (_loadingQueueFinished)
            {
                _guiManager.GuiManager.Value.CloseAllExcept(typeof(TopPanel));
                _guiManager.DoFadeIn.Execute((1f, null));
                await TimeSpan.FromSeconds(1f);
            }

            GC.Collect();

            if (_contract.CurrentLevel.Value != null)
            {
                Destroy(_contract.CurrentLevel.Value.gameObject);
            }

            if (ChangeLvlNum)
            {
                UpdateLvlNum();
            }

            var core = _config.Core;

            _contract.LvlNum.Value = lvlNum;
            _contract.CurrentReward.Value = 0;
            _contract.CurrentLevel.Value = Instantiate(core.levels[lvlNum], transform);

            mainCamera.transform.position = _contract.CurrentLevel.Value.CameraPos.position;
            mainCamera.transform.rotation = _contract.CurrentLevel.Value.CameraPos.rotation;

            if (_loadingQueueFinished)
            {
                _guiManager.DoFadeOut.Execute((1f, null));
                _guiManager.GuiManager.Value.Show<LobbyScreen>(DisplayMode.Screen);
            }
        }

        private void LoadNextLevel()
        {
            var core = _config.Core;
            _data.Money.Value += core.levelRewards[Mathf.Min(core.levelRewards.Count - 1, lvlNum)];
            _data.CurrentLevelNumber.Value++;

            LoadLevel();
        }

        void UpdateLvlNum()
        {
            var core = _config.Core;
            lvlNum = _data.CurrentLevelNumber.Value;
            lvlNum = lvlNum < 0 ? 0 : lvlNum;
            lvlNum = lvlNum >= core.levels.Count ? UnityEngine.Random.Range(core.cyclingLevel, core.levels.Count) : lvlNum;
        }

        
        private void OnLevelCompleted(bool result)
        {
            foreach(var pool in _spawnPool.prefabPools)
            {
                foreach(var spawned in pool.Value.spawned)
                {
                    spawned.SetParent(_spawnPool.transform);
                }

                foreach (var despawned in pool.Value.despawned)
                {
                    despawned.SetParent(_spawnPool.transform);
                }
            }
            _spawnPool.DespawnAll();
            App.Systems.gameObject.SetActive(false); 
            
            if (result)
            {
                _guiManager.GuiManager.Value.Show<WinScreen>(DisplayMode.Screen);
            }
            else
            {
                _guiManager.GuiManager.Value.Show<FailScreen>(DisplayMode.Screen);
            }
        }
        

        private void OnDisable()
        {
            _lifetimeDisposables.Clear();
        }
    }
}