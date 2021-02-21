using System.Collections;
using System.Collections.Generic;
using Core.Game.Configs;
using UnityEngine;
using UnityEngine.AI;

public class FollowTargetGunInfo : BaseGunInfo
{
    protected float _platformRotSpeed { get { return _configValue.PlatformRotSpeed; } }
    protected float _barrelRotSpeed { get { return _configValue.BarrelRotSpeed; } }

    Vector3 _lastShootTagetPos;

    BulletConfig _bulletConfig;

    public override void Init(string configName)
    {
        base.Init(configName);
        _lastShootTagetPos = transform.position;
        _bulletConfig = _config.Core.bulletDict[_bulletName];
    }

    protected override bool CheckHasTarget()
    {
        if(_enemiesOnAgroDist.Count == 0)
        {
            CurrentTarget = null;
            return false;
        }

        if(CurrentTarget == null)
        {
            if (_enemiesOnAgroDist.Count > 1)
            {
                _enemiesOnAgroDist.Sort((a, b) =>
                {
                    var aSqrMag = (a.HitPoint.position - _lastShootTagetPos).sqrMagnitude;
                    var bSqrMag = (b.HitPoint.position - _lastShootTagetPos).sqrMagnitude;
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

    void CheckShoot(Vector3 shootTargetPos, Vector3 shootDir, Quaternion desiredPlatformRot, Quaternion desiredBarrelRot)
    {
        if (Time.time - _lastShootTime >= 1f / FireRate)
        {
            if (Quaternion.Angle(transform.localRotation, desiredPlatformRot) <= 1f - Mathf.Epsilon && Quaternion.Angle(ShootPoint.localRotation, desiredBarrelRot) <= 1f)
            {
                var bullet = _spawnPool.Spawn(_config.Core.bulletDict[_bulletName].PrefabName, ShootPoint.position, Quaternion.identity).GetComponent<BaseBulletInfo>();
                bullet.Init(_bulletName, this, CurrentTarget, BulletMaxDist);

                bullet.Rigidbody.velocity = shootDir * bullet.Speed;

                _lastShootTagetPos = shootTargetPos;
                _lastShootTime = Time.time;

                CheckTaregtHP(bullet.Damage);
            }
        }
    }

    public override void Process()
    {
        if (CheckHasTarget())
        {
            var shootTargetPos = CalcShootTargetPos(out float time);
            var shootDir = (shootTargetPos - ShootPoint.position);

            var enemyTargetPos = CurrentTarget.Target.position;
            var enemyToTargetDir = enemyTargetPos - CurrentTarget.transform.position;
            var enemyToShootTargetPos = shootTargetPos - CurrentTarget.transform.position;

            //если квадрат прогнозируемого попадания дальше, чем квадрат максимального удаления пули
            if (shootDir.sqrMagnitude > BulletMaxDist * BulletMaxDist ||
                //если квадрат от врага до его цели мельше, чем квадрат от врага до прогнозируемого попадания
                (enemyToTargetDir.sqrMagnitude - CurrentTarget.MainCollider.radius * CurrentTarget.MainCollider.radius < enemyToShootTargetPos.sqrMagnitude))
            {
                RemoveTarget(CurrentTarget);
                return;
            }

            shootDir.Normalize();
            Vector3 eulerRot = Quaternion.LookRotation(shootDir).eulerAngles;
            var desiredPlatformRot = Quaternion.Euler(0, eulerRot.y, 0);

           var barrelAngle = _bulletConfig.UseGravity ? - Mathf.Rad2Deg * ShootingUtils.GetAngleOfFlightForTime(_config.Core.bulletDict[_bulletName].Speed, time) : eulerRot.x;
            var desiredBarrelRot = Quaternion.Euler(barrelAngle, 0,  0);

            if (Quaternion.Angle(transform.localRotation, desiredPlatformRot) > 1f)
            {
                transform.localRotation = Quaternion.RotateTowards(transform.rotation, desiredPlatformRot, _platformRotSpeed * Time.deltaTime);
            }

            if (Quaternion.Angle(ShootPoint.localRotation, desiredBarrelRot) > 1f)
            {
                ShootPoint.localRotation = Quaternion.RotateTowards(ShootPoint.localRotation, desiredBarrelRot, _barrelRotSpeed * Time.deltaTime);
            }

            CheckShoot(shootTargetPos, _bulletConfig.UseGravity ? (desiredBarrelRot * shootDir).normalized : shootDir, desiredPlatformRot, desiredBarrelRot);
        }
    }

    protected Vector3 CalcShootTargetPos(out float time)
    {
        return ShootingUtils.LinearIntercept(ShootPoint.position, Vector3.zero, _bulletConfig.Speed,
            CurrentTarget.HitPoint.position, CurrentTarget.HitPoint.forward * CurrentTarget.Speed, out time);
    }
}
