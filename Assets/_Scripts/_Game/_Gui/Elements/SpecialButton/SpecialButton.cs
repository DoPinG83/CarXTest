using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Core.Gui.GuiPool;
using UniRx;

namespace Core.UI.Tutorial
{
    [ExecuteInEditMode]
    [AddComponentMenu("UI/SpecialButton")]
    public class SpecialButton : Button, IPoolable
    {
        /// <summary>
        /// имя кнопки (используется в ButtonsController)
        /// </summary>
        public string TutorialName;
        public bool LockOnBatch = true;
        public bool DesaturateTextOnLock = false;
        public bool HandlesLongPress = false;
        public float LongPressTimeout = 1f;
        public RectTransform ScaleTarget;
        public ButtonClickedEvent onLongPress = new ButtonClickedEvent();

        private readonly List<object> _locks = new List<object>();
        protected bool _held = false;

        protected event Action _beforeClick;

        protected async override void Awake()
        {
            if (ScaleTarget == null)
            {
                ScaleTarget = (RectTransform)transform;
            }
        }

        public void SetButtonName(string tutName)
        {
            TutorialName = tutName;
        }

        // метод, устанавлювающий enabled с контролем источника лока
        // кнопка будет лочится, если хотя бы раз был дернут Lock(true, owner), 
        // но разблокироваться будет только если будет дернут Lock(false, owner) по каждому owner'у
        public void Lock(bool locked, object lockOwner)
        {
            if (locked && !_locks.Contains(lockOwner))
                _locks.Add(lockOwner);
            else if (!locked && _locks.Contains(lockOwner))
                _locks.Remove(lockOwner);
            enabled = _locks.Count <= 0;
        }

        protected override void Start()
        {
            base.Start();
            if (Application.isPlaying)
            {
                //ButtonsController.Instance.RegisterButton(this);
            }

            if (QualitySettings.GetQualityLevel() == 0)
            {
                EnableAnimation = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Application.isPlaying)
            {
                //ButtonsController.Instance.UnregisterButton(this);
            }
            _beforeClick = null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ResetPressed();
            if (Application.isPlaying)
            {
                //ButtonsController.Instance.DropButtonEffects(this);
            }
        }

        public void OnSpawn()
        {
            if (Application.isPlaying)
            {
                //ButtonsController.Instance.RegisterButton(this);
            }
        }

        public void OnUnspawn()
        {
            if (Application.isPlaying)
            {
                //ButtonsController.Instance.UnregisterButton(this);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            bool handleClick = true;
            if (HandlesLongPress)
            {
                // skip click handler if we already invoked LongPressHandler
                if (_held)
                    handleClick = false;
            }

            if (handleClick)
            {
                if (_beforeClick != null) _beforeClick();
                base.OnPointerClick(eventData);
            }
        }

        public void AddBeforeClickListner(Action action)
        {
            _beforeClick += action;
        }

        public void RemoveBeforeClickListner(Action action)
        {
            _beforeClick -= action;
        }

        //ACHTUNG: полная копипаста этого кода в SpecialToggle, не продублировать его разве что можно было множественным наследованием
        #region Animation       //ACHTUNG: факт наличия аниматора на кнопке, каким-то невероятным способом влияет на анимацию (лочил скейл и уползал кнопку)

        public bool EnableAnimation = true;
        public Vector2 AnimationPivot = new Vector2(0.5f, 0.5f);

        protected Vector3 _settingMinAnimationScale = new Vector3(0.95f, 0.95f, 0.95f);
        protected Vector3 _settingNormalAnimationScale = new Vector3(1f, 1f, 1f);
        protected Vector3 _settingMaxAnimationScale = new Vector3(1.05f, 1.05f, 1.05f);

        protected Vector3 _minAnimationScale;
        protected Vector3 _normalAnimationScale;
        protected Vector3 _maxAnimationScale;

        protected bool _pointerPressed;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (HandlesLongPress)
            {
                _held = false;
                Invoke("OnLongPress", LongPressTimeout);
            }

            base.OnPointerDown(eventData);

            if (!EnableAnimation || !this.interactable) return;

            _pointerPressed = true;

            if (_normalAnimationScale == Vector3.zero)
            {
                _normalAnimationScale = _settingNormalAnimationScale;
                _normalAnimationScale.Scale(ScaleTarget.localScale);
                _minAnimationScale = _settingMinAnimationScale;
                _minAnimationScale.Scale(_normalAnimationScale);
                _maxAnimationScale = _settingMaxAnimationScale;
                _maxAnimationScale.Scale(_normalAnimationScale);
            }

            StartScaleAnimation(_normalAnimationScale, _minAnimationScale, 0.05f);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (HandlesLongPress)
                CancelInvoke("OnLongPress");

            base.OnPointerUp(eventData);

            if (!EnableAnimation || !this.interactable) return;

            StartScaleAnimation(_minAnimationScale, _maxAnimationScale, 0.05f, () =>
            {
                StartScaleAnimation(_maxAnimationScale, _normalAnimationScale, 0.05f, () =>
                {
                    _pointerPressed = false;
                });
            });
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            CancelInvoke("OnLongPress");
        }

        private void OnLongPress()
        {
            _held = true;
            onLongPress.Invoke();
        }

        private void StartScaleAnimation(Vector3 from, Vector3 to, float time, Action onFinish = null)
        {
            StopScaleAnimation();

            _scaleAnimation = ScaleAnimation(from, to, time, onFinish);
            StartCoroutine(_scaleAnimation);
        }

        private void StopScaleAnimation()
        {
            if (_scaleAnimation != null)
            {
                StopCoroutine(_scaleAnimation);
                _scaleAnimation = null;
            }
        }

        private IEnumerator _scaleAnimation;
        private IEnumerator ScaleAnimation(Vector3 scaleFrom, Vector3 scaleTo, float time, Action onFinish = null)
        {
            RectTransform rectTransform = ScaleTarget;

            float timePass = 0;
            float progress = 0;

            Vector2 positionFrom = GetCorrectPositionByScale(rectTransform.localScale, scaleFrom);
            Vector2 positionTo = GetCorrectPositionByScale(rectTransform.localScale, scaleTo);
            bool lerpPos = (positionTo - positionFrom).sqrMagnitude >= 0.001f;

            while (progress <= 1)
            {
                progress = timePass/time;

                rectTransform.localScale = Vector3.Lerp(scaleFrom, scaleTo, progress);
                if (lerpPos)
                    rectTransform.anchoredPosition = Vector2.Lerp(positionFrom, positionTo, progress);

                timePass += Time.deltaTime;
                yield return null;
            }

            _scaleAnimation = null;

            if (onFinish != null) onFinish();
        }

        private Vector2 GetCorrectPositionByScale(Vector3 currentScale, Vector3 scale)  // компенсация транслейтом маштабирует за центр без изменения пайвота
        {
            RectTransform rectTransform = ScaleTarget;

            Vector2 correctPosition = currentScale - scale;
            Vector2 size = rectTransform.sizeDelta;
            correctPosition.Scale(size);
            correctPosition.Scale(AnimationPivot - rectTransform.pivot);

            return rectTransform.anchoredPosition + correctPosition;
        }

        private void ResetPressed()
        {
            if (_pointerPressed)
            {
                RectTransform rectTransform = ScaleTarget;
                
                rectTransform.anchoredPosition = GetCorrectPositionByScale(rectTransform.localScale, _normalAnimationScale);
                rectTransform.localScale = _normalAnimationScale;
            }
        }

        #endregion
    }
}
