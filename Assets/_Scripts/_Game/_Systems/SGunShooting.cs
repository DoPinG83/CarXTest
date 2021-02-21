using System.Collections;
using System.Collections.Generic;
using Core.Common.InfoSystems;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;

public class SGunShooting : BaseSystem<BaseGunInfo>
{
    protected override void OnInfoRegistered(BaseGunInfo info)
    {
        info.AgroCollider.OnTriggerEnterAsObservable().Where(val => val.CompareTag("Enemy")).Subscribe(other =>
        {
            info.AddTarget(other.GetComponent<EnemyInfo>());
        }).AddTo(info.LifetimeDisposables).AddTo(LifetimeDisposables);

        info.AgroCollider.OnTriggerExitAsObservable().Where(val => val.CompareTag("Enemy")).Subscribe(other =>
        {
            /*if(info.CurrentTarget == other.GetComponent<EnemyInfo>())
            {
                info.DropTarget();
            }*/
            info.RemoveTarget(other.GetComponent<EnemyInfo>());
        }).AddTo(info.LifetimeDisposables).AddTo(LifetimeDisposables);

        Observable.EveryUpdate().Subscribe(_ =>
        {
            info.Process();
        }).AddTo(info.LifetimeDisposables).AddTo(LifetimeDisposables);
    }
}
