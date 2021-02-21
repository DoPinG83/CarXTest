using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Core.Utils;

namespace Core.Gui
{
    [RequireComponent(typeof(Image))]
    public class CustomHorizontalScroller : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _clickThreshold;
        [SerializeField] private float _clickTime;
        [SerializeField] private float _smallScrollTime = 0.5f;
        [SerializeField] private float _sideClickVelocity;
        [SerializeField] private float _sensitivity;
        [SerializeField, Range(0, 1)] private float _inertia;
        [SerializeField] private float _highVelocityThreshold;
        [SerializeField] private float _nextCardVelocityThreshold;
        [SerializeField] private float _dragForce;
        [SerializeField] private bool _useMagnet;
        [SerializeField] private float _magnetPower;
        [SerializeField] private float _decelerationRate = 1;
        [SerializeField] private float _minDistanceToDrag;
        [SerializeField] private float _minVelocityToDrag;
        [SerializeField, Range(0, 1)] private float _magnetPosition;

        [Range(0, 1)] public float NormalizedValue;

        public float Velocity => _velocity;

        public bool HighVelocity => Mathf.Abs(_velocity) >= _highVelocityThreshold && !_dragging;

        public event Action<Vector3> Click = delegate { };
        public event Action<float> ScrollToNext = delegate { };

        private float dragDelta
        {
	        get { return 1920f * _dragDelta/ (Screen.width * Screen.dpi); }
	        set { _dragDelta = value; }
        }

        public void SetMagnetPosition(float pos)
        {
            _magnetPosition = Mathf.Clamp01(pos);
            _useMagnet = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _startClickTime = Time.time;
            _startClickPos = eventData.position;

            _useMagnet = false;
            _dragging = true;
            dragDelta = 0;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;

            var deltaTime = Time.time - _startClickTime;
            var deltaPos = (eventData.position - _startClickPos).sqrMagnitude;

            if (deltaTime <= _clickTime && deltaPos <= _clickThreshold)
                Click(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragDelta = eventData.delta.y < eventData.delta.x ? eventData.delta.magnitude : eventData.delta.x;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragDelta = 0;
//
//            var delta = eventData.delta.y < eventData.delta.x ? eventData.delta.magnitude : eventData.delta.x;
//            _velocity = - delta * _dragForce / Time.deltaTime;
	        _velocity *= _dragForce;
            var deltaTime = Time.time - _startClickTime;

            if (Mathf.Abs(_velocity) < _highVelocityThreshold && deltaTime < _smallScrollTime)
            {
                ScrollToNext(Mathf.Sign(_velocity));
                _useMagnet = true;
            }

            _dragging = false;
        }

        //private void Start()
        //{
        //    _customEventSystem = ControllersManager.Instance.GetController<CustomEventSystem>();

        //    _customEventSystem.OnDrag += OnDrag;
        //    _customEventSystem.OnEndDrag += OnEndDrag;
        //}

        private void LateUpdate()
        {

            if (_dragging)
            {
                _velocity = - dragDelta;
                NormalizedValue = Mathf.Repeat(NormalizedValue + _velocity * Time.deltaTime * _sensitivity, 1);
                dragDelta = 0;
            }
            else
            {
                var diff = _magnetPosition - NormalizedValue;
                var dist = Mathf.Abs(diff) >= 0.5f
                    ? diff - Mathf.Sign(diff)
                    : diff;

                var deceleration = 0f;
                var inertia = _inertia;
                if (!HighVelocity && _useMagnet)
                {
                    inertia /= 8;
                    deceleration = dist * _decelerationRate;
                }

                _velocity -= _velocity * Time.deltaTime / inertia - deceleration;

                NormalizedValue = Mathf.Repeat(NormalizedValue + _velocity * Time.deltaTime, 1);
            }
        }

        private float CalculateMagneticOffset()
        {
            if (!_useMagnet)
                return 0;

            var diff = _magnetPosition - NormalizedValue;
            var dist = Mathf.Abs(diff) >= 0.5f
                ? diff - Mathf.Sign(diff)
                : diff;

            return Mathf.MoveTowards(0, dist, _magnetPower);
        }

        private float _velocity;
        private float _startClickTime;
        private Vector2 _startClickPos;
        private float _dragDelta;
        private bool _dragging;
    }
}