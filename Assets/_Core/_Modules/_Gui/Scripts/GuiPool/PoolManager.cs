using System.Collections.Generic;
using UnityEngine;

namespace Core.Gui.GuiPool
{
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;

        private static PoolManager GetInstance()
        {
            if (_instance == null)
            {
                var go = new GameObject("PoolManager");
                _instance = go.AddComponent<PoolManager>();
            }
            return _instance;
        }

        public static GameObject Spawn(GameObject prefab)
        {
            return GetInstance().SpawnInternal(prefab);
        }

        public static void Unspawn(GameObject go)
        {
            GetInstance().UnspawnInternal(go);
        }

        public static Pool GetPool(GameObject prefab)
        {
            return GetInstance().GetPoolInternal(prefab);
        }

        private readonly Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();

        private GameObject SpawnInternal(GameObject prefab)
        {
            var pool = GetPoolInternal(prefab);
            return pool.Spawn();
        }

        private void UnspawnInternal(GameObject go)
        {
            var poolObject = go.GetComponent<PoolComponent>();
            if (poolObject == null)
            {
                GameObject.Destroy(go);
            }
            else
            {
                poolObject.Pool.Unspawn(go);
            }
        }

        private Pool GetPoolInternal(GameObject prefab)
        {
            Pool result = null;
            if (!_pools.TryGetValue(prefab, out result))
            {
                result = new Pool(prefab, transform);
                _pools.Add(prefab, result);
            }
            return result;
        }
    }
}
