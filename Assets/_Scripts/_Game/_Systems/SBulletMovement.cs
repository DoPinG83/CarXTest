using System.Collections;
using System.Collections.Generic;
using Core.Common.InfoSystems;
using UniRx;
using UnityEngine;

public class SBulletMovement : BaseSystem<BaseBulletInfo>
{
    protected override void OnInfoRegistered(BaseBulletInfo info)
    {
        Observable.EveryUpdate().Subscribe(_ =>
        {
            info.Move();
        }).AddTo(info.LifetimeDisposables)
        .AddTo(LifetimeDisposables);
    }
}
