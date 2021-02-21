using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Utils;
using Object = UnityEngine.Object;
using System.Linq;
using PathologicalGames;

namespace Core.Gui
{
    /// <summary>
    /// Фабрика ГУИ объектов
    /// </summary>
    public class GuiFactory
    {
        protected readonly Dictionary<Type, GameObject> _reusingObjects;
        protected SpawnPool _spawnPool;
        
        public GuiFactory()
        {
            _reusingObjects = new Dictionary<Type, GameObject>();
            _spawnPool = PoolManager.Pools["GuiPool"];
        }

        /// <summary>
        /// Создает геймобъект ГУИ по классу контроллера
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual GameObject CreateGuiObject<T>() where T : GuiControllerBase
        {
            return CreateObject(typeof(T));
        }

        /// <summary>
        /// Создает геймобъект ГУИ по классу контроллера
        /// </summary>
        /// <param name="guiType">тип контроллера</param>
        /// <returns></returns>
        public virtual GameObject CreateGuiObject(Type guiType)
        {
            return CreateObject(guiType);
        }

        private GameObject CreateObject(Type guiType)
        {
            string prefabName = guiType.ToString().Split('.').Last();
            var guiTransform = _spawnPool.Spawn(prefabName);
            if(guiTransform == null)
            {
                CoreLog.LogErrorFormat(string.Format("No such gui in GuiPool: {0}", guiType));
            }

            return guiTransform.gameObject;
        }

        /// <summary>
        /// Создает геймобъект модальной подложки
        /// </summary>
        /// <returns></returns>
        public virtual GameObject CreateModalObject()
        {
            return CreateObject(typeof(Modal));
        }
    }
}
