using UnityEngine;
using UnityEngine.UI;
using System;
using Core.Utils;

namespace Core.Gui.Canvases
{
    [RequireComponent(typeof(UnityEngine.Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasBase : MonoBehaviour
    {
        protected CanvasScaler _canvasScaler;

        private RectTransform _canvasTransform;
        public RectTransform CanvasRectTransform
        {
            get { return _canvasTransform; }
        }

        public Canvas CanvasObject;

        public Rect CanvasRect
        {
            get { return _canvasTransform.rect; }
        }

        protected virtual void Awake()
        {
            _canvasTransform = transform as RectTransform;
            _canvasScaler = GetComponent<CanvasScaler>();
            CanvasObject = GetComponent<Canvas>();
            if (_canvasScaler == null)
            {
                CoreLog.LogError("No CanvasScaler found");
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        protected virtual CanvasScaler.ScaleMode GetUiScaleMode()
        {
            return CanvasScaler.ScaleMode.ConstantPixelSize;
        }

        protected virtual void Start()
        {
            if (_canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
            {
                _canvasScaler.scaleFactor = (float)Screen.width / 1080f;
            }
        }
    }
}