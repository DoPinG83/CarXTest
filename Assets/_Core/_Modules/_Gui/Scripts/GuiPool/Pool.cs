using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Utils;

namespace Core.Gui.GuiPool
{
    public class Pool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;

        private readonly Stack<GameObject> _freeObjects;
        private readonly HashSet<GameObject> _spawnedObjects;

        public Pool(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;

            _freeObjects = new Stack<GameObject>();
            _spawnedObjects = new HashSet<GameObject>();

        }

        public GameObject Spawn()
        {
            if (_freeObjects.Count == 0)
            {
                Allocate(1);
            }

            GameObject go = _freeObjects.Pop();

            go.SetActive(true);
            //PoolableUtils.NotifySpawn(go);

            _spawnedObjects.Add(go);
            return go;
        }

        public T SpawnInParent<T>(Transform parent, bool saveOffests = false) where T : Component
        {
            GameObject obj = Spawn();

            Transform transform = obj.transform;
            transform.SetParent(parent);
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            if (saveOffests)
            {
                ((RectTransform)transform).offsetMin = ((RectTransform)_prefab.transform).offsetMin;
                ((RectTransform)transform).offsetMax = ((RectTransform)_prefab.transform).offsetMax;
            }
            T component = obj.GetComponent<T>();

            return component;
        }


        public void Unspawn(GameObject go)
        {
            if (!_spawnedObjects.Contains(go))
            {
                CoreLog.LogError("Trying to unspawn object from another pool");
                return;
            }

            go.transform.SetParent(_parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;

            //PoolableUtils.NotifyUnspawn(go);
            go.SetActive(false);

            _spawnedObjects.Remove(go);
            _freeObjects.Push(go);
        }

        public void Allocate(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                var go = GameObject.Instantiate(_prefab, _parent);
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.SetActive(false);

                var poolObject = go.AddComponent<PoolComponent>();
                poolObject.Pool = this;

                _freeObjects.Push(go);
            }
        }
    }
}
