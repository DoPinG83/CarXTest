using System;
using UnityEngine;

namespace Core.Gui.GuiPool
{
    public class PoolComponent : MonoBehaviour
    {
        [NonSerialized]
        public Pool Pool;

        public void Unspawn()
        {
            Pool.Unspawn(gameObject);
        }

    }
}
