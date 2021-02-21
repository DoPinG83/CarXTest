using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Utils.GuiEffect
{
    public class GuiEffectState : MonoBehaviour
    {
        private enum EffectType
        {
            Custom,
            Gray,
            Desaturate
        }

        private class MaterialSettings
        {
            public Material material;
            public Color color;
        }

        private class EffectData : MaterialSettings
        {
            public EffectType type;
            public object token;
        }

        private Graphic _graphic;
        private MaterialSettings _original;
        private readonly Dictionary<object, EffectData> _effects
            = new Dictionary<object, EffectData>();

        public bool Initialized { get; private set; }
        public bool Destroyed { get; private set; }

        public void Init(Graphic graphic)
        {
            Initialized = true;
            _graphic = graphic;
            _original = new MaterialSettings { color = graphic.color, material = graphic.material };
        }

        public void GrayEffect(object effectOwner, Color grayColor)
        {
            //если эффект уже применен, не применяем его снова
            if (_effects.ContainsKey(effectOwner))
                return;
            var effectData = new EffectData { token = effectOwner, type = EffectType.Gray };
            effectData.color = grayColor;
            effectData.material = _original.material; // материал не меняется для этого эффекта
            TryApplyEffect(effectData);
            _effects.Add(effectOwner, effectData);
        }

        public void Desaturate(object token, Material desaturateMaterial, bool desaturateTextColor)
        {
            if (_effects.ContainsKey(token))
                return;
            var effectData = new EffectData { token = token, type = EffectType.Desaturate };
            Color color = _original.color;
            var material = _original.material;
            if (_graphic is Text)
            {
                if (desaturateTextColor)
                    color = new Color(color.r * 0.25f, color.g * 0.25f, color.b * 0.25f, color.a);
            }
            else
                material = desaturateMaterial;
            effectData.color = color;
            effectData.material = material;
            TryApplyEffect(effectData);
            _effects.Add(token, effectData);
        }

        public void SetCustomMaterial(object effectOwner, Material material)
        {
            if (_effects.ContainsKey(effectOwner))
                return;
            var effectData = new EffectData { token = effectOwner, type = EffectType.Custom };
            effectData.color = _original.color; // пока без изменения цвета
            effectData.material = material;
            TryApplyEffect(effectData);
            _effects.Add(effectOwner, effectData);
        }

        // если это самый приоритетнйы эффект - применяем его
        void TryApplyEffect(EffectData effectData)
        {
            if (!_effects.All(t => t.Value.type < effectData.type))
                return;
            //CoreLog.LogError(string.Format("Apply **NEW** {0} to {1}", effectData.type, gameObject.name));
            _graphic.color = effectData.color;
            _graphic.material = effectData.material;
        }

        public void SetOriginalColor(Color color)
        {
            _original.color = color;
        }

        public void Reset(object effectOwner)
        {
            if (!_effects.ContainsKey(effectOwner))
                return;
            _effects.Remove(effectOwner);
            if (_effects.Count == 0)
            {
                //CoreLog.LogError(string.Format("**RESET** all effects for {0}", gameObject.name));
                _graphic.color = _original.color;
                _graphic.material = _original.material;
                Destroy(this);
                Destroyed = true;
                return;
            }
            var effectData = _effects.OrderByDescending(t => t.Value.type).First().Value;
            //CoreLog.LogError(string.Format("Apply **DEFERRED** effect after reset {0} for {1}", effectData.type, gameObject.name));
            _graphic.color = effectData.color;
            _graphic.material = effectData.material;
        }
    }
}
