using UnityEngine;
using UnityEngine.UI;

namespace Core.Gui.Canvases
{
    /// <summary>
    /// Объект в канвасе, в котором отрисовываются все UI, принадлежащие объектам:
    /// Прогресс бары, надписи, иконки и т. д. Все объекты рисуются в одном слое, а иих размер не
    /// зависит от приближения камеры.
    /// </summary>
    public class DynamicCanvas : CanvasBase
    {
        [SerializeField]
        private RectTransform _persistentContainer;

        [SerializeField]
        private RectTransform _temporaryContainer;


        // TODO: убрать синглтон
        public static DynamicCanvas Instance { get; private set; }

        public RectTransform PersistentContainer
        {
            get { return _persistentContainer; }
        }

        public RectTransform TemporaryContainer
        {
            get { return _temporaryContainer; }
        }

        public Vector2 Resolution
        {
            get { return _canvasScaler.referenceResolution; }
        }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public void SetCanvasesEnabled(bool enabled)
        {
            transform.GetChildren(z => z.activeSelf, child =>
            {
                CanvasSetEnabled(child, enabled);
            });
        }

        private void CanvasSetEnabled(GameObject obj, bool enabled)
        {
            var canvas = obj.GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = enabled;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
