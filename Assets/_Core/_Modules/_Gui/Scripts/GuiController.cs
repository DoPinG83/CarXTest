using System;
using Core.Gui.Canvases;
using Core.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System.Collections;
using UniRx;

namespace Core.Gui
{
    /// <summary>
    /// Базовый класс для контроллера ГУИ
    /// </summary>
    public class GuiController : GuiControllerBase
    {
        public enum AnimationType
        {
            None,
            Animator,
            GentleAnimation,
            CrossFade,
            PopupTweener
        }

        protected enum GentleAnimationType
        {
            Open,
            Close
        }

        [SerializeField]
        protected AnimationType _animationType = AnimationType.CrossFade;

        [SerializeField]
        protected GentleAnimation _gentleAnimation;

        [SerializeField]
        private GuiPopupTweener _popupTweener;

        [SerializeField]
        protected float _crossFadeSpeed = 4;

        [SerializeField, Range(0.1f, 10f)]
        protected float _openSpeed = 1f;
        [SerializeField, Range(0.1f, 10f)]
        protected float _closeSpeed = 1f;

        [SerializeField]
        protected bool forceNoAutoSorting;

        protected GuiManager _guiManager;

        protected bool _isDialogOpened;
        protected bool _closing;
        protected bool _animationDisabled;

        public int SortingOrder { get; private set; }

        public bool IsModal { get; set; }

        private readonly CompositeDisposable _lifetimeDisposables = new CompositeDisposable();
        private readonly CompositeDisposable _buttonBindings = new CompositeDisposable();

        protected CompositeDisposable LifetimeDisposables => _lifetimeDisposables;
        protected CompositeDisposable ButtonBindings => _buttonBindings;

        protected bool firstEnable = true;

        protected virtual async void OnEnable()
        {
            if (firstEnable)
            {
                firstEnable = false;
                return;
            }

            Init();
        }

        protected virtual void Start()
        {
            if (_closing) return;

            if (_gentleAnimation == null)
            {
                _gentleAnimation = gameObject.GetComponent<GentleAnimation>();
                if (_gentleAnimation == null)
                {
                    foreach (Transform child in gameObject.transform)
                        if ((_gentleAnimation = child.gameObject.GetComponent<GentleAnimation>()) != null)
                            break;
                }
            }
        }

        protected async virtual void Init()
        {
        }

        public override void Dispose()
        {
            _lifetimeDisposables.Clear();
            _buttonBindings.Clear();

            _isDialogOpened = false;
            _closing = false;
            base.Dispose();
        }

        public void ResetSortingOrder()
        {
            if (_canvas == null)
                return;
            _canvas.sortingOrder = StaticCanvas.Instance.CanvasObject.sortingOrder + 2;
        }

        public override void Setup(DisplayMode displayMode)
        {
            base.Setup(displayMode);
            if (_canvas == null)
                return;

            SortingOrder = _canvas.sortingOrder;

            // TODO: hack used to ignore auto sorting calcs, used in FirstLoginScreen;
            if (!forceNoAutoSorting)
            {
                SortingOrder = _guiManager.GetCurrentMaxSortingOrder() + 2; // +2 из-за Modal, который подкладывается под окно и у него -1 от своего окна, такие пироги
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = StaticCanvas.Instance.CanvasObject.sortingOrder + SortingOrder;
            }

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();
        }

        public float GetAnimationTime(string animationName, float speed, bool activateAnimation = false)
        {
            var animators = GetComponentsInChildren<Animator>();
            var maxTime = 0f;
            foreach (var anim in animators)
            {
                anim.speed = speed;
                var time = 0f;
                var ac = anim.runtimeAnimatorController;    //Get Animator controller
                for (int i = 0; i < ac.animationClips.Length; i++)                 //For all animations
                {
                    if (ac.animationClips[i].name == animationName)        //If it has the same name as your clip
                    {
                        time = ac.animationClips[i].length;

                        if (activateAnimation)
                            anim.SetTrigger(animationName);

                        if (maxTime < time)
                            maxTime = time;

                        break;
                    }
                }
            }
            return maxTime / speed;
        }

        protected void Animate(string animationName, float speed, Action done)
        {
            StopCurrentAnimationCoroutine();
            currentAnimationCoroutine = StartCoroutine(WaitForAnimationFinished(animationName, speed,() =>
            {
                currentAnimationCoroutine = null;
                done();
            }));
        }

        IEnumerator WaitForAnimationFinished(string animationName, float speed, Action done)
        {
            yield return new WaitForSeconds(GetAnimationTime(animationName, speed, true));
            done?.Invoke();
        }

        protected Coroutine currentAnimationCoroutine;
        protected virtual void StopCurrentAnimationCoroutine()
        {
            if (currentAnimationCoroutine != null)
                StopCoroutine(currentAnimationCoroutine);

            currentAnimationCoroutine = null;
        }

        public override void AnimateOpen()
        {
            void onOpenFinish()
            {
                _isDialogOpened = true;
                OnOpen?.Invoke();
            }

            if (_animationDisabled )
            {
                onOpenFinish();
                return;
            }

            switch(_animationType)
            {
                case AnimationType.Animator:
                    Animate("open", _openSpeed, () =>
                    {
                        onOpenFinish();
                    });
                    break;

                case AnimationType.GentleAnimation:
                    StopCurrentAnimationCoroutine();
                    if (_gentleAnimation != null)
                    {
                        _gentleAnimation.Stop(); //Break current animation;
                        _gentleAnimation.PlayClipByIndex((int)GentleAnimationType.Open, s =>
                        {
                            onOpenFinish();
                        });
                    }
                    else
                    {
                        CoreLog.LogWarning(string.Format("No GentleAnimation for {0}", GetType()));
                        onOpenFinish();
                    }
                    break;

                case AnimationType.CrossFade:
                    StopCurrentAnimationCoroutine();
                    CanvasGroup.alpha = 0;
                    currentAnimationCoroutine = CrossFadeAlpha(1, _crossFadeSpeed, 0, () =>
                    {
                        currentAnimationCoroutine = null;
                        onOpenFinish();
                    });
                    break;

                case AnimationType.PopupTweener:
                    if (_popupTweener != null)
                    {
                        _popupTweener.StartOpenTween();
                    }
                    else
                    {
                        CoreLog.LogWarning(string.Format("No PopupFX for {0}", GetType()));
                    }
                    onOpenFinish();
                    break;

                case AnimationType.None:
                    onOpenFinish();
                    break;
            }
        }

        public void DisableAnimation()
        {
            _animationDisabled = true;
        }

        public override void AnimateClose()
        {
            void onCloseFinish()
            {
                OnClose?.Invoke();
            }

            _closing = true;
            if (_animationDisabled)
            {
                onCloseFinish();
                return;
            }

            switch (_animationType)
            {
                case AnimationType.Animator:
                    //Already in animation
                    if (currentAnimationCoroutine != null)
                    {
                        StopCurrentAnimationCoroutine();
                        OnClose?.Invoke();
                    }
                    else
                    {
                        Animate("close", _closeSpeed, () =>
                        {
                            onCloseFinish();
                        });
                    }
                    break;

                case AnimationType.GentleAnimation:
                    StopCurrentAnimationCoroutine();
                    if (_gentleAnimation != null)
                    {
                        _gentleAnimation.Stop(); //Break current animation;
                        _gentleAnimation.PlayClipByIndex((int)GentleAnimationType.Close, s => onCloseFinish());
                    }
                    else
                    {
                        CoreLog.LogWarning(string.Format("No GentleAnimation for {0}", GetType()));
                        onCloseFinish();
                    }
                    break;

                case AnimationType.PopupTweener:
                case AnimationType.CrossFade:
                    StopCurrentAnimationCoroutine();
                    CanvasGroup.alpha = 1;
                    currentAnimationCoroutine = CrossFadeAlpha(0, _crossFadeSpeed, 0, onCloseFinish);
                    break;

                case AnimationType.None:
                    onCloseFinish();
                    break;
            }
        }

        /// <summary>
        /// Устанавливает общие данные, нужные гуишкам
        /// </summary>
        /// <param name="guiManager"></param>
        public virtual void SetEnvironment(GuiManager guiManager)
        {
            _guiManager = guiManager;
        }

        private void OnDisable()
        {
            _lifetimeDisposables.Clear();
            _buttonBindings.Clear();
        }

        public override void Close()
        {
            _guiManager.Close(this);
        }

        public virtual void CloseImmediate()
        {
            _lifetimeDisposables.Clear();
            _buttonBindings.Clear();

            _guiManager.Close(this, false);

            OnClose?.Invoke();
        }

        public override bool OnAndroidBackButton()
        {
            if (_displayMode == DisplayMode.Persistent ||
                _displayMode == DisplayMode.Screen)
            {
                return false;
            }
            Close();
            return true;
        }
    }
}
