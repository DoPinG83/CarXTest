using System;
using System.Collections;
using Core.Gui.GuiPool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Gui.Carousel
{
    [SelectionBase]
    [RequireComponent(typeof(RectTransform))]
    public abstract class CarouselScroller : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement
    {
        public enum MovementType
        {
            Unrestricted, // Unrestricted movement -- can scroll forever
            Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
            Clamped, // Restricted movement where it's not possible to go past the edges
        }

        [Serializable]
        public class ScrollRectEvent : UnityEvent<Vector2> { }

        [SerializeField]
        protected RectTransform _content;
        public RectTransform Content { get { return _content; } set { _content = value; } }
        protected RectOffset _contentPadding;

        [SerializeField]
        protected MovementType _movement = MovementType.Elastic;
        public MovementType Movement { get { return _movement; } set { _movement = value; } }

        [SerializeField]
        private float _elasticity = 0.1f; // Only used for MovementType.Elastic
        public float Elasticity { get { return _elasticity; } set { _elasticity = value; } }

        [SerializeField]
        private ConditionalFloat _inertia = new ConditionalFloat { Condition = true, Value = 0.135f };
        public bool Inertia { get { return _inertia.Condition; } set { _inertia.Condition = value; } }
        public float DecelerationRate { get { return _inertia.Value; } set { _inertia.Value = value; } }

        [SerializeField]
        private float _scrollSensitivity = 1.0f;
        public float ScrollSensitivity { get { return _scrollSensitivity; } set { _scrollSensitivity = value; } }

        [SerializeField]
        private bool _maintainTransform = false;

        [SerializeField]
        private bool _looped = false;
        public bool Looped { get { return _looped; } set { _looped = value; } }

        public bool Locked { get; set; }

        [SerializeField]
        private ScrollRectEvent _valueChangedEvent = new ScrollRectEvent();

        public ScrollRectEvent ValueChangedEvent { get { return _valueChangedEvent; } set { _valueChangedEvent = value; } }

        // The offset from handle position to mouse down position
        private Vector2 _pointerStartLocalCursor = Vector2.zero;
        private Vector2 _contentStartPosition = Vector2.zero;

        private RectTransform _viewRect;

        private Vector2? _savedPosition;
        private bool _resetSavedPositionOnDrag;

        public void SavePosition(bool resetSavedPositionOnDrag)
        {
            _savedPosition = _content.anchoredPosition;
            _resetSavedPositionOnDrag = resetSavedPositionOnDrag;
        }

        public void RestorePosition(bool clearAfter)
        {
            if (_savedPosition == null)
                return;
            _content.anchoredPosition = _savedPosition.Value;
            if (clearAfter)
                _savedPosition = null;
        }

        public RectTransform ViewRect
        {
            get
            {
                if (_viewRect == null)
                    _viewRect = (RectTransform)transform;
                return _viewRect;
            }
        }

        protected Bounds _contentBounds;
        protected Bounds _viewBounds;

        private Vector2 _velocity;
        public Vector2 Velocity { get { return _velocity; } set { _velocity = value; } }

        private bool _dragging;

        private Vector2 _prevPosition = Vector2.zero;
        private Bounds _prevContentBounds;
        private Bounds _prevViewBounds;
        [NonSerialized]
        private bool _hasRebuiltLayout = false;

        private Vector2? _dragCursor = null;

        protected Func<Vector2?> _swapForwardHandler;
        protected Func<Vector2?> _swapBackwardHandler;
        private Coroutine _scrollCoroutine;
        private Action _scrollCallback;

        public void SetSwapHandlers(Func<Vector2?> swapForward, Func<Vector2?> swapBackward)
        {
            _swapForwardHandler = swapForward;
            _swapBackwardHandler = swapBackward;
        }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing != CanvasUpdate.PostLayout)
                return;

            UpdateBounds();
            UpdateScrollbars(Vector2.zero);
            UpdatePrevData();
            _hasRebuiltLayout = true;
        }

        public void Clear()
        {
            if (_content != null)
            {
                _content.GetChildren(go => true, child =>
                {
                    var poolObject = child.GetComponent<PoolComponent>();

                    if (poolObject != null)
                    {
                        poolObject.Unspawn();
                    }
                    else
                    {
                        child.transform.SetParent(null);
                        GameObject.Destroy(child);
                    }
                });
            }
        }

        public void ScrollTo(RectTransform rect, Action cb)
        {
            if (!gameObject.activeSelf)
                return;
            _scrollCallback = cb;
            if (_scrollCoroutine != null)
                StopCoroutine(_scrollCoroutine);
            _scrollCoroutine = StartCoroutine(OnScrollFrameEnded(rect));
        }

        private IEnumerator OnScrollFrameEnded(RectTransform rect)
        {
            yield return new WaitForEndOfFrame();
            if (rect != null && rect.gameObject != null)
            {
                //CoreLog.LogError("rect.sizeDelta.y " + rect.sizeDelta.y + " rect.anchoredPosition.y " + rect.anchoredPosition.y + " rect.pivot.y - 1 " + (rect.pivot.y - 1));
                ScrollContentToRect(rect);

                bool swapped = false;
                do { swapped = TrySwap(); } while (swapped);
            }
            _scrollCoroutine = null;
            if (_scrollCallback != null)
                _scrollCallback();
            _scrollCallback = null;
        }

        protected abstract void ScrollContentToRect(RectTransform rect);

        protected override void Awake()
        {
            if (_maintainTransform)
                return;
            _content.pivot = new Vector2(0.5f, 0.5f);
            _content.anchorMin = _content.anchorMax = new Vector2(0.5f, 0.5f);
            _content.anchoredPosition = Vector2.zero;
        }

        protected override void Start()
        {
            _contentPadding = _content.GetComponent<LayoutGroup>().padding;
        }

        protected override void OnDisable()
        {
            _hasRebuiltLayout = false;
            base.OnDisable();
        }

        public override bool IsActive()
        {
            return base.IsActive() && _content != null;
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!_hasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        public virtual void StopMovement()
        {
            _velocity = Vector2.zero;
        }

        protected virtual Vector2 GetScrollDelta(PointerEventData data)
        {
            return data.scrollDelta;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (Locked || !IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = GetScrollDelta(data);
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;

            Vector2 position = _content.anchoredPosition;
            position += delta * _scrollSensitivity;
            if (_movement == MovementType.Clamped)
                position += CalculateOffset(position - _content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            _velocity = Vector2.zero;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {       
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (Locked || !IsActive())
                return;

            UpdateBounds();

            _pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out _pointerStartLocalCursor);
            _contentStartPosition = _content.anchoredPosition;
            _dragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            _dragging = false;
            _dragCursor = null;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (_resetSavedPositionOnDrag && _savedPosition != null)
                _savedPosition = null;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (Locked || !IsActive())
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;
            _dragCursor = localCursor;

            UpdateBounds();

            var pointerDelta = localCursor - _pointerStartLocalCursor;
            Vector2 position = _contentStartPosition + pointerDelta;

            // Offset to get Content into place in the view.
            Vector2 offset = CalculateOffset(position - _content.anchoredPosition);
            position += offset;
            if (_movement == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, _viewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, _viewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }

        public void OnCarouselItemSwap(Vector2 contentDelta)
        {
            var newPos = new Vector2(
                _content.anchoredPosition.x + contentDelta.x,
                _content.anchoredPosition.y + contentDelta.y);
            _content.anchoredPosition = newPos;
            _contentStartPosition = newPos;
            if (_dragCursor != null)
                _pointerStartLocalCursor = _dragCursor.Value;
            UpdatePrevData();
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            CorrectAnchoredPosition(ref position);

            if (position != _content.anchoredPosition)
            {
                _content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        protected virtual void CorrectAnchoredPosition(ref Vector2 position) { }

        bool firstTime = true;
        protected virtual void LateUpdate()
        {
            if (firstTime) {
                firstTime = false;
                return;
            }

            if (_content == null || _content.childCount == 0)
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);
            if (!_dragging && (offset != Vector2.zero || _velocity != Vector2.zero))
            {
                Vector2 position = _content.anchoredPosition;
                for (int axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and Content has an offset from the view.
                    if (_movement == MovementType.Elastic && offset[axis] != 0)
                    {
                        float speed = _velocity[axis];
                        position[axis] = Mathf.SmoothDamp(_content.anchoredPosition[axis], _content.anchoredPosition[axis] + offset[axis], ref speed, _elasticity, Mathf.Infinity, deltaTime);
                        _velocity[axis] = speed;
                    }
                    // Else move Content according to Velocity with deceleration applied.
                    else if (Inertia)
                    {
                        _velocity[axis] *= Mathf.Pow(DecelerationRate, deltaTime);
                        if (Mathf.Abs(_velocity[axis]) < 1)
                            _velocity[axis] = 0;
                        position[axis] += _velocity[axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any Velocity.
                    else
                    {
                        _velocity[axis] = 0;
                    }
                }

                if (_velocity != Vector2.zero)
                {
                    if (_movement == MovementType.Clamped)
                    {
                        offset = CalculateOffset(position - _content.anchoredPosition);
                        position += offset;
                    }

                    SetContentAnchoredPosition(position);
                }
            }

            if (_dragging && Inertia)
            {
                Vector3 newVelocity = (_content.anchoredPosition - _prevPosition) / deltaTime;
                _velocity = Vector3.Lerp(_velocity, newVelocity, deltaTime * 10);
            }

            if (_viewBounds != _prevViewBounds || _contentBounds.size != _prevContentBounds.size || (_content.anchoredPosition - _prevPosition).sqrMagnitude > 0.1f)
            {
                UpdateScrollbars(offset);
                OnScrollValueChanged(normalizedPosition);
                UpdatePrevData();
            }
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            if (_valueChangedEvent != null)
                _valueChangedEvent.Invoke(value);
            TrySwap();
        }

        private bool TrySwap()
        {
            Vector2 containerSize = ViewRect.rect.size;
            Vector2? swapedItemSize;
            bool swapped = TrySwap(new Vector2(-containerSize.x * _content.anchorMin.x, -containerSize.y * _content.anchorMin.y),
                                   new Vector2(containerSize.x * (1f - _content.anchorMax.x), containerSize.y * (1f - _content.anchorMax.y)),
                                   out swapedItemSize);
            if (swapped)
                OnCarouselItemSwap(swapedItemSize.Value);
            return swapped;
        }

        protected abstract bool TrySwap(Vector2 scrollRectMin, Vector2 scrollRectMax, out Vector2? swapedItemSize);

        public void UpdatePrevData()
        {
            if (_content == null)
                _prevPosition = Vector2.zero;
            else
                _prevPosition = _content.anchoredPosition;
            _prevViewBounds = _viewBounds;
            _prevContentBounds = _contentBounds;
        }

        protected virtual void UpdateScrollbars(Vector2 offset)
        {
        }

        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (_contentBounds.size.x <= _viewBounds.size.x)
                    return (_viewBounds.min.x > _contentBounds.min.x) ? 1 : 0;
                return (_viewBounds.min.x - _contentBounds.min.x) / (_contentBounds.size.x - _viewBounds.size.x);
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (_contentBounds.size.y <= _viewBounds.size.y)
                    return (_viewBounds.min.y > _contentBounds.min.y) ? 1 : 0;
                ;
                return (_viewBounds.min.y - _contentBounds.min.y) / (_contentBounds.size.y - _viewBounds.size.y);
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        protected void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            // How much the Content is larger than the view.
            float hiddenLength = _contentBounds.size[axis] - _viewBounds.size[axis];
            // Where the position of the lower left corner of the Content bounds should be, in the space of the view.
            float contentBoundsMinPosition = _viewBounds.min[axis] - value * hiddenLength;
            // The new Content localPosition, in the space of the view.
            float newLocalPosition = _content.localPosition[axis] + contentBoundsMinPosition - _contentBounds.min[axis];

            Vector3 localPosition = _content.localPosition;
            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                _content.localPosition = localPosition;
                _velocity[axis] = 0;
                UpdateBounds();
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        private void UpdateBounds()
        {
            _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            _contentBounds = GetBounds();

            if (_content == null)
                return;

            // Make sure Content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the Content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when Content is *larger* than view.
            // We use the pivot of the Content rect to decide in which directions the Content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the Content.
            Vector3 contentSize = _contentBounds.size;
            Vector3 contentPos = _contentBounds.center;
            Vector3 excess = _viewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (_content.pivot.x - 0.5f);
                contentSize.x = _viewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (_content.pivot.y - 0.5f);
                contentSize.y = _viewBounds.size.y;
            }

            _contentBounds.size = contentSize;
            _contentBounds.center = contentPos;
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        private Bounds GetBounds()
        {
            if (_content == null)
                return new Bounds();

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = ViewRect.worldToLocalMatrix;
            _content.GetWorldCorners(m_Corners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        protected abstract Vector2 CalculateOffset(Vector2 delta);

        public virtual void GraphicUpdateComplete() { }

        public virtual void LayoutComplete() { }
    }
}
