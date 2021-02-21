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

public class EnemyInfo : BaseInfo<EnemyInfo>
{
    [SerializeField] NavMeshAgent _agent;

    [SerializeField] CapsuleCollider _mainCollider;
    public CapsuleCollider MainCollider => _mainCollider;

    [SerializeField] Transform _hitPoint;
    public Transform HitPoint => _hitPoint;

    public int MaxHP { get{ return _configValue.HP; } }
    public float Speed { get { return _configValue.Speed; } }

    public int CurrHP { get; private set; }
    public Transform Target { get; private set; }
    public int ForecastedDamage { get; private set; }

    public readonly ReactiveCommand<EnemyInfo> Killed = new ReactiveCommand<EnemyInfo>();
    public readonly ReactiveCommand<EnemyInfo> ReachedTarget = new ReactiveCommand<EnemyInfo>();

    private SpawnPool _spawnPool;
    private CConfig _config;

    private EnemyConfig _configValue;

    protected override async UniTask OnInfoEnable()
    {
        _spawnPool = PoolManager.Pools["SpawnPool"];
        _config = await App.Common.ResolveAsync<CConfig>();
    }

    public void Init(string configName, Transform target)
    {
        _configValue = _config.Core.enemyDict[configName];

        CurrHP = MaxHP;
        ForecastedDamage = 0;
        Target = target;

        NavMeshPath path = new NavMeshPath();
        var found = _agent.CalculatePath(Target.position, path);
        if(found)
        {
            transform.forward = Target.position - transform.position;
            _agent.SetDestination(Target.position);
            _agent.speed = Speed;
        }
    }

    public bool ApplyDamage(int damage)
    {
        bool killed = false;
        CurrHP -= damage;
        if(CurrHP <= 0)
        {
            killed = true;

            Killed.Execute(this);

            if (_spawnPool.IsSpawned(transform))
            {
                _spawnPool.Despawn(transform);
            }
        }

        return killed;
    }

    public void AddForecastedDamage(int damage)
    {
        ForecastedDamage += damage;
    }

    public void TargetReached()
    {
        ReachedTarget.Execute(this);

        if (_spawnPool.IsSpawned(transform))
        {
            _spawnPool.Despawn(transform);
        }
    }
}
