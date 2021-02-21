using System;
using UnityEngine;
using Core.UI;

namespace Core.Gui
{
    /// <summary>
    /// Контроллер префаба ГУИ
    /// </summary>
    public abstract class GuiControllerBase : UICanvasGroupBase, IDisposable
    {
        [SerializeField]
        private bool _dontDestroyWhenClose;

        protected DisplayMode _displayMode;
        protected Canvas _canvas;
        public bool IsHidden { get; protected set; }
        public event Action<bool> HiddenStateChanged = delegate { };

        public Action OnOpen = delegate { };
        public Action OnClose = delegate { };

        public bool DontDestroyWhenClose { get => _dontDestroyWhenClose;
            protected set => _dontDestroyWhenClose = value;
        }
    
        public virtual void Setup(DisplayMode displayMode)
        {
            _displayMode = displayMode;
            if (!InOwnCanvas())
                return;

            _canvas = gameObject.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
                _canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;
            }
        }

        /// <summary>
        /// Оверрайдить для отписки от коллбеков и тп деинициализации, вызывается автоматически GuiManager'ом
        /// </summary>
        public virtual void Dispose() { }

        public virtual void AnimateOpen()
        {
            OnOpen?.Invoke();
        }
        public virtual void AnimateClose()
        {
            OnClose.Invoke();
        }

        /// <summary>
        /// когда экран скрывается/показывается, при показе одного диалога поверх другого
        /// </summary>
        /// <param name="hidden"></param>
        public virtual void Hide(bool hidden)
        {
            IsHidden = hidden;
            if (this == null)
                return;

            if (_canvas == null)
                ((RectTransform)transform).anchoredPosition = hidden ? new Vector2(5000, 0) : Vector2.zero;
            else
                _canvas.enabled = !hidden;
            OnHide(hidden);
            HiddenStateChanged(hidden);
        }

        protected virtual void OnHide(bool hidden) { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>не проспукать дальше по стеку нажатие кнопки</returns>
        public virtual bool OnAndroidBackButton()
        {
            return true;
        }

        public virtual bool InOwnCanvas()
        {
            return _displayMode == DisplayMode.DialogAddFront ||
                   _displayMode == DisplayMode.DialogHideScreen ||
                   _displayMode == DisplayMode.DialogCloseOthers || 
                   _displayMode == DisplayMode.Dialog;
        }

        public abstract void Close();
    }
}

