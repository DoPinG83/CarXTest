using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryGunInfo : BaseGunInfo
{

    public override void Process()
    {
        if (Time.time - _lastShootTime >= 1f / FireRate)
        {
            if (!CheckHasTarget())
                return;

            var bullet = _spawnPool.Spawn(_config.Core.bulletDict[_bulletName].PrefabName, ShootPoint.position, ShootPoint.rotation, ShootPoint).GetComponent<BaseBulletInfo>();
            bullet.Init(_bulletName, this, CurrentTarget, BulletMaxDist);

            _lastShootTime = Time.time;

            CheckTaregtHP(bullet.Damage);
        }
    }
}
