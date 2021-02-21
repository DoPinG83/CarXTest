
using System;
using UnityEngine;
using Core.Utils;

namespace Core.UI
{
    [Serializable]
    public class SpriteData
    {
        public string Id;
        public Sprite Sprite;
    }

    [Serializable]
    public class SpritesMap
    {
        [SerializeField] private SpriteData[] _spritesMap;

        [SerializeField][HideInInspector]
        private bool _foldout;

        public Sprite GetSprite(string id)
        {
            foreach (var item in _spritesMap)
            {
                if (item.Id == id)
                    return item.Sprite;
            }
            CoreLog.LogWarning(string.Format("No sprite with key: {0}", id));
            return null;
        }

        public Sprite this[string key]
        {
            get
            {
                return GetSprite(key);
            }
        }
    }
}
