using System;
using System.Collections;
using Core.Utils.Desaturate;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Utils.GuiEffect
{
    class GuiEffect : MonoBehaviour
    {
        public Material desaturateMaterial = null;
        public Color grayColor = Color.gray;

        private static GuiEffect _instance;
        public static GuiEffect Instance { get { return _instance; } }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public static IEnumerator AnimateSlide(RectTransform panel, Vector2 targetPosition, float animationTime, Action onFinish)
        {
            float timePass = 0;
            while (timePass <= animationTime)
            {
                timePass += Time.deltaTime;
                panel.anchoredPosition = Vector3.Lerp(panel.anchoredPosition, targetPosition, timePass / animationTime);

                yield return null;
            }

            panel.anchoredPosition = targetPosition;

            if (onFinish != null)
                onFinish();
        }

        public static void GrayEffect(GameObject go, object effectOwner)
        {
            var graphic = go.GetComponent<Graphic>();
            if (graphic != null)
            {
                var state = GetEffectState(go, graphic, true);
                state.GrayEffect(effectOwner, _instance.grayColor);
            }
            foreach (Transform child in go.transform)
                GrayEffect(child.gameObject, effectOwner);
        }

        public static void Desaturate(GameObject go, object effectOwner, bool desaturateTextColor, Material customMaterial = null)
        {
            if (go.GetComponent<SkipDesaturate>() != null)
                return;
            var graphic = go.GetComponent<Graphic>();
            if (graphic != null)
            {
                var state = GetEffectState(go, graphic, true);
                state.Desaturate(effectOwner, customMaterial ?? _instance.desaturateMaterial, desaturateTextColor);
            }
            foreach (Transform child in go.transform)
            {
                if(child.GetComponent<TMP_SubMeshUI>() == null)
                    Desaturate(child.gameObject, effectOwner, desaturateTextColor, customMaterial);
            }
        }

        public static void SetCustomMaterial(GameObject go, object effectOwner, Material material, bool recursive = false)
        {
            var graphic = go.GetComponent<Graphic>();
            if (graphic != null && go.GetComponent<Text>() == null)
            {
                var state = GetEffectState(go, graphic, true);
                state.SetCustomMaterial(effectOwner, material);
            }
            if (!recursive)
                return;
            foreach (Transform child in go.transform)
                SetCustomMaterial(child.gameObject, effectOwner, material, recursive);
        }

        public static void Reset(GameObject go, object effectOwner)
        {
            var graphic = go.GetComponent<Graphic>();
            if (graphic != null)
            {
                var state = GetEffectState(go, graphic, false);
                if (state != null)
                    state.Reset(effectOwner);
                else
                    return;
            }
            foreach (Transform child in go.transform)
                Reset(child.gameObject, effectOwner);
        }

        public static void DesaturateEnable(bool enable, GameObject go, object effectOwner, bool desaturateTextColor, Material customMaterial = null)
        {
            if (enable)
                Desaturate(go, effectOwner, desaturateTextColor, customMaterial);
            else
                Reset(go, effectOwner);
        }

        private static GuiEffectState GetEffectState(GameObject go, Graphic graphic, bool createIfNotExists)
        {
            var states = go.GetComponents<GuiEffectState>();
            foreach (var state in states)
            {
                if (state.Destroyed)
                    continue;
                if (!state.Initialized)
                    state.Init(graphic);
                return state;
            }
            if (!createIfNotExists)
                return null;
            var effectState = go.AddComponent<GuiEffectState>();
            effectState.Init(graphic);
            return effectState;
        }
    }
}
