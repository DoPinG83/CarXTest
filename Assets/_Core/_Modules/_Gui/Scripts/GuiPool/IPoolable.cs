using UnityEngine;

namespace Core.Gui.GuiPool
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnUnspawn();
    }

    public static class PoolableUtils
    {
        public static void NotifySpawn(GameObject root)
        {
            var poolables = root.GetComponentsInChildren<IPoolable>(true);
            foreach (var poolable in poolables)
            {
                poolable.OnSpawn();
            }
        }

        public static void NotifyUnspawn(GameObject root)
        {
            var poolables = root.GetComponentsInChildren<IPoolable>(true);
            foreach (var poolable in poolables)
            {
                poolable.OnUnspawn();
            }
        }
    }
}
