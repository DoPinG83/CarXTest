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
using UnityEngine.AI;

public abstract class BaseGunInfo : BaseInfo<BaseGunInfo>
{
    [SerializeField] Transform _shootPoint;
    public Transform ShootPoint => _shootPoint;

    [SerializeField] SphereCollider  _agroCollider;
    public SphereCollider AgroCollider => _agroCollider;

    protected string _bulletName { get { return _configValue.BulletName; } }
    public float FireRate { get { return _configValue.FireRate; } }
    public float BulletMaxDist { get { return _configValue.BulletMaxDist; } }

    public EnemyInfo CurrentTarget { get; protected set; }

    protected SpawnPool _spawnPool;
    protected CConfig _config;
    protected CGame _game;

    protected GunConfig _configValue;
    protected float _lastShootTime;

    protected List<EnemyInfo> _enemiesOnAgroDist = new List<EnemyInfo>();

    protected override async UniTask OnInfoEnable()
    {
        _spawnPool = PoolManager.Pools["SpawnPool"];
        _config = await App.Common.ResolveAsync<CConfig>();
        _game = await App.Common.ResolveAsync<CGame>();
    }

    public virtual void Init(string configName)
    {
        _configValue = _config.Core.gunDict[configName];
        _agroCollider.radius = _configValue.ArgoDist;

        _enemiesOnAgroDist.Clear();
        DropTarget();
    }


    protected virtual bool CheckHasTarget()
    {
        if (_enemiesOnAgroDist.Count == 0)
        {
            CurrentTarget = null;
            return false;
        }

        if (CurrentTarget == null)
        {
            if (_enemiesOnAgroDist.Count > 1)
            {
                _enemiesOnAgroDist.Sort((a, b) =>
                {
                    var aSqrMag = (a.HitPoint.position - _shootPoint.position).sqrMagnitude;
                    var bSqrMag = (b.HitPoint.position - _shootPoint.position).sqrMagnitude;
                    if (aSqrMag > bSqrMag)
                        return 1;
                    else if (aSqrMag < bSqrMag)
                        return -1;
                    else
                        return 0;
                });
            }

            CurrentTarget = _enemiesOnAgroDist[0];
        }

        return true;
    }

    public virtual void AddTarget(EnemyInfo enemy)
    {
        if (!_enemiesOnAgroDist.Contains(enemy))
        {
            _enemiesOnAgroDist.Add(enemy);
            enemy.Killed.Subscribe(RemoveTarget).AddTo(LifetimeDisposables).AddTo(enemy.LifetimeDisposables);
            enemy.ReachedTarget.Subscribe(RemoveTarget).AddTo(LifetimeDisposables).AddTo(enemy.LifetimeDisposables);
        }
    }

    public virtual void RemoveTarget(EnemyInfo enemy)
    {

        if (_enemiesOnAgroDist.Contains(enemy))
        {
            _enemiesOnAgroDist.Remove(enemy);
        }

        if (CurrentTarget == enemy)
        {
            DropTarget();
        }
    }

    public virtual void DropTarget()
    {
        CurrentTarget = null;
        CheckHasTarget();
    }

    protected void CheckTaregtHP(int damage)
    {
        CurrentTarget.AddForecastedDamage(damage);
        if (CurrentTarget.ForecastedDamage >= CurrentTarget.MaxHP || CurrentTarget.CurrHP <= damage)
        {
            RemoveTarget(CurrentTarget);
        }
    }

    abstract public void Process();
}
