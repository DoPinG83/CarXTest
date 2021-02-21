using UnityEngine;

namespace Core.Game.Systems
{
    using System;
    using System.Linq;
    using Core.Common.InfoSystems;
    using Core.Game;
    using Core.Game.Infos;
    using Core.Game.Managers;
    using PathologicalGames;
    using UniRx;
    using UniRx.Async;

    public class SLevelState : BaseSystem<LevelInfo>
    {
        private SpawnPool _spawnPool;
        private CConfig _config;
        private CGame _game;

        private CompositeDisposable _levelDisposables = new CompositeDisposable();

        private LevelInfo _info;

        protected override async UniTask OnSystemEnable()
        {
            _spawnPool = PoolManager.Pools["SpawnPool"];
            _config = await App.Common.ResolveAsync<CConfig>();
            _game = await App.Common.ResolveAsync<CGame>();
        }

        protected override void OnInfoRegistered(LevelInfo info)
        {
            _info = info;
            foreach (var towerPlacement in info.TowerPlacements)
            {
                var tower = _spawnPool.Spawn(towerPlacement.TowerName, towerPlacement.transform.position, Quaternion.identity, info.transform);

                var gun = _spawnPool.Spawn(_config.Core.gunDict[towerPlacement.GunName].PrefabName,
                    towerPlacement.GunPosition.position, Quaternion.identity, tower).GetComponent<BaseGunInfo>();
                gun.Init(towerPlacement.GunName);
            }

            Observable.Interval(TimeSpan.FromSeconds(info.EnemySpawnPeriod))
            .StartWith(0)
            .Subscribe(_ =>
            {
                int randomIndex = UnityEngine.Random.Range(0, info.SpawnTargetDict.Count);

                var enemy = _spawnPool.Spawn(_config.Core.enemyDict[info.GetEnemy(info.EnemiesSpawned)].PrefabName,
                    info.SpawnTargetDict.Keys.ElementAt(randomIndex).position, Quaternion.identity, info.transform).GetComponent<EnemyInfo>();

                enemy.Init(info.GetEnemy(info.EnemiesSpawned), info.SpawnTargetDict.Values.ElementAt(randomIndex).transform);

                enemy.Killed.Subscribe(__ =>
                {
                    info.EnemyKilled();
                    LevelCompletedCheck();
                }).AddTo(LifetimeDisposables).AddTo(enemy.LifetimeDisposables);

                enemy.ReachedTarget.Subscribe(___ =>
                {
                    info.EnemyTargetReached();
                    LevelCompletedCheck();
                }).AddTo(LifetimeDisposables).AddTo(enemy.LifetimeDisposables);

                info.EnemySpawned();
                if(info.EnemiesSpawned == info.EnemiesCount)
                {
                    _levelDisposables.Clear();
                }
            }).AddTo(_levelDisposables);
        }

        private void LevelCompletedCheck()
        {
            if (_info.EnemiesCount == _info.EnemiesSpawned && _info.EnemiesTargetReached + _info.EnemiesKilled == _info.EnemiesCount)
            {
                _game.OnLevelCompleted.Execute(_info.EnemiesKilled >= _info.EnemiesKillToPass);
            }
        }

        protected override void OnSystemDisable()
        {
            base.OnSystemDisable();
            _levelDisposables.Clear();
        }
    }
}