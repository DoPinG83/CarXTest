using System;
using System.Collections;
using UnityEngine;
using Core.Utils;

namespace Core.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(CanvasGroup))]
    public class UICanvasGroupBase : BaseGameObject
    {


        public enum TransitionType
        {
            MoveTowards, Lerp,
        }

        [SerializeField]
        float m_HideAlpha = 0f;

        public RectTransform ThisRectTransform
        {
            get { return _rectTransform == null ? _rectTransform = GetComponent<RectTransform>() : _rectTransform; }
        }

        private RectTransform _rectTransform;

        [SerializeField]
        CanvasGroup m_CanvasGroup;
        public CanvasGroup CanvasGroup { get { return m_CanvasGroup; } }

        public bool Visible { get { return gameObject.activeSelf && m_CanvasGroup.alpha > m_HideAlpha; } }

        public bool InTransition
        {
            get
            {
                return 
                    gameObject.activeSelf &&
                    m_CanvasGroup.alpha > m_HideAlpha && 
                    m_CanvasGroup.alpha < 1f;
            }
        }

        public virtual void SetVisible(bool value, bool affectRaycasts = false, bool hideСompletely = false)
        {
	        var alpha = hideСompletely ? 0f : m_HideAlpha;

            CanvasGroup.alpha = value ? 1f : alpha;
            if (affectRaycasts)
                CanvasGroup.blocksRaycasts = value;
        }

        public Coroutine CrossFadeAlpha(float to, float speed, float delay, Action finished = null)
        {
            return CrossFadeAlpha(to, speed, delay, TransitionType.MoveTowards, finished);
        }

        public Coroutine CrossFadeAlpha(float to, float speed, float delay, TransitionType transitionType, Action finished = null)
        {
            StopAllCoroutines();
            return StartCoroutine(CrossFadeAlphaCoroutine(to, speed, delay, transitionType, finished));
        }

        IEnumerator CrossFadeAlphaCoroutine(float to, float speed, float delay, TransitionType transitionType, Action finished)
        {
            yield return new WaitForSeconds(delay);

            var time = Time.time;
            while (!AlmostEquals(CanvasGroup.alpha, to))
            {
                CanvasGroup.alpha = UpdateValue(CanvasGroup.alpha, to, speed, transitionType);
                yield return null;
            }

            CanvasGroup.alpha = to;
            finished?.Invoke();
        }

        bool AlmostEquals(float value1, float value2)
        {
            return Math.Abs(value1 - value2) <= 0.01f;
        }

        float UpdateValue(float current, float to, float speed, TransitionType transitionType)
        {
            switch (transitionType)
            {
                case TransitionType.Lerp:
                    return Mathf.Lerp(CanvasGroup.alpha, to, Time.deltaTime * speed);
                case TransitionType.MoveTowards:
                    return Mathf.MoveTowards(CanvasGroup.alpha, to, Time.deltaTime * speed);
            }
            CoreLog.LogErrorFormat("Invalid transition type {0}", transitionType);
            return 0;
        }
    }
}