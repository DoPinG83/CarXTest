using System.Collections;
using System.Collections.Generic;
using Core.Common.InfoSystems;
using Core.Game;
using Core.Game.Configs;
using Core.Game.Managers;
using PathologicalGames;
using UniRx;
using UniRx.Async;
using UnityEngine;

public class BaseBulletInfo : BaseInfo<BaseBulletInfo>
{
	public float Speed { get { return _configValue.Speed; } }
	public int Damage { get { return _configValue.Damage; } }

	public Rigidbody Rigidbody { get; private set; }
	public BaseGunInfo Owner { get; private set; }

	protected SpawnPool _spawnPool;
	protected CConfig _config;

	protected BulletConfig _configValue;
	protected EnemyInfo _target;
	protected Vector3 _initPos;
	protected float _maxSqrDist;

	protected override async UniTask OnInfoEnable()
	{
		_spawnPool = PoolManager.Pools["SpawnPool"];
		_config = await App.Common.ResolveAsync<CConfig>();

		Rigidbody = GetComponent<Rigidbody>();
	}

	public virtual void Init(string configName, BaseGunInfo owner, EnemyInfo target, float maxDist)
	{
		_configValue = _config.Core.bulletDict[configName];

		_target = target;

		_initPos = transform.position;
		_maxSqrDist = maxDist * maxDist;
		Owner = owner;
		Rigidbody.useGravity = _configValue.UseGravity;
	}

	public virtual void Move()
	{
		CheckMaxDist();
	}

	protected bool CheckMaxDist()
    {
		if (Vector3.SqrMagnitude(transform.position - _initPos) > _maxSqrDist)
		{
			_spawnPool.Despawn(transform);
			return false;
		}

		return true;
	}
}
