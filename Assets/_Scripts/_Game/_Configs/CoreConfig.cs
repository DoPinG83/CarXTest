namespace Core.Game.Configs
{
    using UnityEngine;
    using System.Collections.Generic;
    using Core.Game.Infos;
    using RotaryHeart.Lib.SerializableDictionary;

    [CreateAssetMenu(fileName = "CoreConfig", menuName = "Configs/CoreConfig")]
    public class CoreConfig : ScriptableObject
    {
        [Header("Levels")] 
        public List<LevelInfo> levels;
        public List<int> levelRewards;
        public int cyclingLevel;

        [Header("Gameplay")]
        public BulletConfigDict bulletDict;
        public EnemyConfigDict enemyDict;
        public GunConfigDict gunDict;
    }

    [System.Serializable]
    public class BulletConfig
    {
        public string PrefabName;
        public float Speed;
        public int Damage;
        public bool UseGravity;
    }

    [System.Serializable]
    public class EnemyConfig
    {
        public string PrefabName;
        public float Speed;
        public int HP;
    }

    [System.Serializable]
    public class GunConfig
    {
        public string PrefabName;
        public string BulletName;
        public float FireRate;
        public float ArgoDist;
        public float BulletMaxDist;
        public float PlatformRotSpeed;
        public float BarrelRotSpeed;
    }

    [System.Serializable]
    public class BulletConfigDict : SerializableDictionaryBase<string, BulletConfig> { };

    [System.Serializable]
    public class EnemyConfigDict : SerializableDictionaryBase<string, EnemyConfig> { };

    [System.Serializable]
    public class GunConfigDict : SerializableDictionaryBase<string, GunConfig> { };
}
