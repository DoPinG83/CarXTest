using System.Collections;
using System.Collections.Generic;
using Core.Common.InfoSystems;
using PathologicalGames;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;

public class SEnemyMovement : BaseSystem<EnemyInfo>
{
    SpawnPool _spawnPool;

    protected override async UniTask OnSystemEnable()
    {
        _spawnPool = PoolManager.Pools["SpawnPool"];
    }

    protected override void OnInfoRegistered(EnemyInfo info)
    {
        info.MainCollider.OnTriggerEnterAsObservable().Subscribe(other =>
        {
            if (other.CompareTag("EnemyTarget"))
            {
                info.TargetReached();
            }
            else if (other.CompareTag("PlayerBullet"))
            {
                var bullet = other.GetComponent<BaseBulletInfo>();

                info.ApplyDamage(bullet.Damage);

                if (_spawnPool.IsSpawned(bullet.transform))
                {
                    _spawnPool.Despawn(bullet.transform);
                }
            }
        }).AddTo(LifetimeDisposables).AddTo(info.LifetimeDisposables);
    }
}
