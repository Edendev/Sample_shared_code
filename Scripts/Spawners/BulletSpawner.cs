using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Combat.Bullets;
using Game.ScriptableObjects.Combat.Bullets;
using Game.MonoBehaviours.Managers;
using Game.Interfaces;

namespace Game.MonoBehaviours.Spawners
{
    public class BulletSpawner : ObjectSpawner<Bullet, BulletSO>
    {
        public override void Initialize(BulletSO _bulletSO, int _poolSize)
        {
            base.Initialize(_bulletSO, _poolSize);
            objectPool = GameManager.Pooling.GetObjectPool<Bullet>(objectSO.GetBullet(), poolSize, false);
        }

        public override Bullet Spawn()
        {
            Bullet newBullet = objectPool.GetObject(null, transform.position, Vector3.zero);
            newBullet.Initialize(objectSO);
            return newBullet;
        }

        public override void TearDown()
        {
            if (objectSO != null) GameManager.Pooling.ReturnObjectPool(objectSO.GetBullet(), poolSize, false);
        }
    }
}
