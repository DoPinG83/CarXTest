using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Utils;

namespace Core.UI
{
    [Serializable]
    public class ColorData
    {
        //
        // Fields
        //
        public string Id;

        [ColorUsage(true, true)]
        public Color Color;
    }

    [Serializable]
    public class ColorsMap
    {
        [SerializeField] private ColorData[] _colorsMap;

        [SerializeField]
        [HideInInspector]
        private bool _foldout;

        public Color GetColor(string id, Color? defaultColor = null)
        {
            foreach (var item in _colorsMap)
            {
                if (item.Id == id)
                    return item.Color;
            }
            CoreLog.LogError("No color with key: " + id);
            return defaultColor.HasValue ? defaultColor.Value : Color.white;
        }

        public Color this[string key]
        {
            get
            {
                return GetColor(key);
            }
        }
    }
}
