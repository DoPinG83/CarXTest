using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class HomingBulletInfo : BaseBulletInfo
{
    public override void Init(string configName, BaseGunInfo owner, EnemyInfo target, float maxDist)
    {
        base.Init(configName, owner, target, maxDist);

		_target.Killed.Subscribe(_ =>
		{
			if (_spawnPool.IsSpawned(transform))
			{
				_spawnPool.Despawn(transform);
			}
		}).AddTo(LifetimeDisposables).AddTo(target.LifetimeDisposables);

		target.ReachedTarget.Subscribe(_ =>
		{
			if (_spawnPool.IsSpawned(transform))
			{
				_spawnPool.Despawn(transform);
			}
		}).AddTo(LifetimeDisposables).AddTo(target.LifetimeDisposables);
	}

    public override void Move()
    {
		if (!CheckMaxDist())
			return;

		var translation = (_target.HitPoint.position - transform.position).normalized * Speed * Time.deltaTime;
		transform.Translate(translation);
	}
}
