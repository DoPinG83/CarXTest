using System.Collections.Generic;
using Core.Common.InfoSystems;
using Core.Game.Components;
using RotaryHeart.Lib.SerializableDictionary;
using UniRx.Async;
using UnityEngine;

namespace Core.Game.Infos
{
    public class LevelInfo : BaseInfo<LevelInfo>
    {
        [SerializeField] public List<TowerPlacement> TowerPlacements = new List<TowerPlacement>();
        [SerializeField] public SpawnTargetDict SpawnTargetDict;

        [SerializeField] List<string> _enemies;
        public int EnemiesCount { get { return _enemies.Count; } }

        [SerializeField] float _enemySpawnPeriod;
        public float EnemySpawnPeriod => _enemySpawnPeriod;

        [SerializeField] int _enemiesKillToPass;
        public int EnemiesKillToPass => _enemiesKillToPass;

        [SerializeField] Transform _cameraPos;
        public Transform CameraPos => _cameraPos;

        public int EnemiesKilled { get; private set; }
        public int EnemiesSpawned { get; private set; }
        public int EnemiesTargetReached { get; private set; }

        public string GetEnemy(int i)
        {
            return _enemies[i];
        }

        public void EnemyKilled()
        {
            EnemiesKilled++;
        }

        public void EnemySpawned()
        {
            EnemiesSpawned++;
        }

        public void EnemyTargetReached()
        {
            EnemiesTargetReached++;
        }
    }

    [System.Serializable]
    public class SpawnTargetDict : SerializableDictionaryBase<Transform, Collider> { };
}
