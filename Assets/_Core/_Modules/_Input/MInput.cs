namespace Core.Common.Modules.Input {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Extensions;
    using Misc;
    using UI;
    using UniRx;
    using UniRx.Triggers;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class CInput : Contract<CInput> {

        [Output] public ReactiveCommand OnTap      = new ReactiveCommand();
        [Output] public ReactiveCommand OnLeftTap  = new ReactiveCommand();
        [Output] public ReactiveCommand OnRightTap = new ReactiveCommand();
        [Output] public ReactiveCommand OnTimedTap = new ReactiveCommand();

        [Output] public ReactiveCommand<Vector2> OnTapWithPosition = new ReactiveCommand<Vector2>();

        [Output] public ReactiveCommand OnSwipeUp    = new ReactiveCommand();
        [Output] public ReactiveCommand OnSwipeDown  = new ReactiveCommand();
        [Output] public ReactiveCommand OnSwipeLeft  = new ReactiveCommand();
        [Output] public ReactiveCommand OnSwipeRight = new ReactiveCommand();

        [Output] public ReactiveCommand        OnHoldStarted  = new ReactiveCommand();
        [Output] public ReactiveCommand<float> OnHoldReleased = new ReactiveCommand<float>();
        [Output] public ReactiveProperty<bool> IsPressed      = new ReactiveProperty<bool>();


        [Output] public ReactiveProperty<Vector2> CurrentPointerMovement = new ReactiveProperty<Vector2>();

        [Output] public ReactiveProperty<bool>    StaticJoystickInputActive     = new ReactiveProperty<bool>();
        [Output] public ReactiveProperty<Vector2> StaticJoystickInputStartPoint = new ReactiveProperty<Vector2>();
        [Output] public ReactiveProperty<Vector2> CurrentStaticJoystickInput    = new ReactiveProperty<Vector2>();

        [Output] public ReactiveProperty<bool>    DynamicJoystickInputActive     = new ReactiveProperty<bool>();
        [Output] public ReactiveProperty<Vector2> DynamicJoystickInputStartPoint = new ReactiveProperty<Vector2>();
        [Output] public ReactiveProperty<Vector2> CurrentDynamicJoystickInput    = new ReactiveProperty<Vector2>();

        [Output] public ReactiveProperty<RectTransform> Rect = new ReactiveProperty<RectTransform>();

        [Output] public ReactiveCommand<Vector2> OnDragBegin = new ReactiveCommand<Vector2>();
        [Output] public ReactiveCommand<Vector2> OnDrag      = new ReactiveCommand<Vector2>();
        [Output] public ReactiveCommand<Vector2> OnDragEnd   = new ReactiveCommand<Vector2>();
    }

    public class MInput : MonoBehaviour {

        public enum ActiveAxes {
            OnlyX,
            OnlyY,
            BothXY,
        }

        private CInput _contract;

        private readonly CompositeDisposable _lifetimeDisposables = new CompositeDisposable();

        [SerializeField] private Image _inputImage;

        [Header("Dynamic joystick input settings")]
        [Header("Input settings")]
        [SerializeField] private ActiveAxes _dJoysticActiveAxes = ActiveAxes.BothXY;
        [SerializeField] private float _dJoystickMinInputDistanceFromAnchor = 10;
        [SerializeField] private float _dJoystickMaxInputDistanceFromAnchor = 150;
        [SerializeField] private float _anchorPointMaxDistance              = 180;
        [SerializeField] private bool  _useSmoothAnchorMovement = true;
        [SerializeField] private float _anchorPointVelocity = 7;
        [Header("Static joystick input settings")]
        [SerializeField] private ActiveAxes _sJoysticActiveAxes = ActiveAxes.BothXY;
        [SerializeField] private float         _sJoystickMinInputDistanceFromAnchor = 10;
        [SerializeField] private float         _sJoystickMaxInputDistanceFromAnchor = 150;
        [SerializeField] private bool          _usePresetAnchorPosition = false;
        [SerializeField] private RectTransform _presetAnchor;
        [SerializeField] private RectTransform _inputRectTransform;
        [Header("Swipe settings")]
        [Range(0, 1)] [SerializeField] private float _horizontalSwipeSensitivity = 0.7f;
        [Range(0, 1)] [SerializeField] private float _verticalSwipeSensitivity = 0.7f;
        [Range(0, 1)] [SerializeField] private float _secondHorizontalSwipeSensitivity = 0.55f;
        [Range(0, 1)] [SerializeField] private float _secondVerticalSwipeSensitivity = 0.55f;
        [SerializeField]               private float _swipeResetDistance = 100;
        [Header("Tap settings")]
        [SerializeField] private float _timedTapTime = 0.3f;
        [SerializeField] private float _timedTapDistance = 40f;

#region Swipe parameters
        private enum SwipeDirection {
            None,
            Right,
            Left,
            Up,
            Down,
        }

        private float          _pointerPositionTimeAgoTime = 0.5f;
        private int            _horizontalSwipesCount;
        private int            _verticalSwipesCount;
        private float          _swipeStartPositionX;
        private float          _swipeStartPositionY;
        private SwipeDirection _swipeHorizontalDirection;
        private SwipeDirection _swipeVerticalDirection;

        private readonly ReactiveProperty<Vector2> _pointerPositionTimeAgo                       = new ReactiveProperty<Vector2>();
        private readonly ReactiveProperty<Vector2> _currentPointerPosition                       = new ReactiveProperty<Vector2>();
        private readonly CompositeDisposable       _absenceOfPointerHorizontalMovementDisposable = new CompositeDisposable();
        private readonly CompositeDisposable       _absenceOfPointerVerticalMovementDisposable   = new CompositeDisposable();
#endregion

        private readonly CompositeDisposable _absenceOfPointerMovementDisposable = new CompositeDisposable();

        private void OnEnable() {
            _contract = this.Get<CInput>();

            BindSwipeInput();
            BindHoldInput();
            BindTapInput();
            BindJoystickInput();
            BindPointerMovementInput();
            BindDragDirectionalInput();
            BindOnDragEvents();


            _contract.Rect.Value = _inputRectTransform;

            Modules.All.Register(_contract);
        }

        private void OnDisable() {
            _lifetimeDisposables.Clear();
            _absenceOfPointerHorizontalMovementDisposable.Clear();
            _absenceOfPointerVerticalMovementDisposable.Clear();
            _absenceOfPointerMovementDisposable.Clear();
        }

#region Tap input logic
        private void BindTapInput() {
            float tapTime = 0f;
            Vector2 tapPosition = Vector2.zero;

            _inputImage.OnPointerDownAsObservable()
                .Subscribe(pointerEventData => {
                    tapTime = Time.time;
                    tapPosition = pointerEventData.position;

                    _contract.OnTap.Execute();
                    _contract.OnTapWithPosition.Execute(pointerEventData.position);
                    if (pointerEventData.position.x > Screen.width / 2) _contract.OnRightTap.Execute();
                    else _contract.OnLeftTap.Execute();
                })
                .AddTo(_lifetimeDisposables);

            _inputImage.OnPointerUpAsObservable().Subscribe(pointerEventData => {
                    tapTime = Time.time - tapTime;
                    float tapDistance = (pointerEventData.position - tapPosition).magnitude;

                    if (tapTime <= _timedTapTime && tapDistance <= _timedTapDistance) {
                        _contract.OnTimedTap.Execute();
                    }
                })
                .AddTo(_lifetimeDisposables);
        }
#endregion

#region Hold input logic
        private void BindHoldInput() {
            float holdStartTime = 0;
            _inputImage.OnPointerDownAsObservable()
                .Subscribe(pointerEventData => {
                    _contract.OnHoldStarted.Execute();
                    _contract.IsPressed.Value = true;
                    holdStartTime = Time.time;
                })
                .AddTo(_lifetimeDisposables);

            _inputImage.OnPointerUpAsObservable()
                .Subscribe(pointerEventData => {
                    _contract.OnHoldReleased.Execute(Time.time - holdStartTime);
                    _contract.IsPressed.Value = false;
                })
                .AddTo(_lifetimeDisposables);
        }
#endregion

#region Swipe input logic
        private void BindSwipeInput() {
            float prevX = 0;
            float prevY = 0;
            _absenceOfPointerHorizontalMovementDisposable.Clear();
            _absenceOfPointerVerticalMovementDisposable.Clear();

            _inputImage.OnDragAsObservable()
                .Subscribe(pointerEventData => {
                    // horizontal swipes
                    float newX = pointerEventData.position.x;
                    if (newX > prevX && _swipeHorizontalDirection != SwipeDirection.Right) ResetHorizontalSwipe();
                    else if (newX < prevX && _swipeHorizontalDirection != SwipeDirection.Left) ResetHorizontalSwipe();
                    _swipeHorizontalDirection = pointerEventData.delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                    prevX = newX;
                    // vertical swipes
                    float newY = pointerEventData.position.y;
                    if (newY > prevY && _swipeVerticalDirection != SwipeDirection.Up) ResetVerticalSwipe();
                    else if (newY < prevY && _swipeVerticalDirection != SwipeDirection.Down) ResetVerticalSwipe();
                    _swipeVerticalDirection = pointerEventData.delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                    prevY = newY;
                })
                .AddTo(_lifetimeDisposables);

            _inputImage.OnDragAsObservable()
                .Select(pointerEventData => pointerEventData.position)
                .Delay(TimeSpan.FromSeconds(_pointerPositionTimeAgoTime))
                .Subscribe(newPos => _pointerPositionTimeAgo.Value = newPos)
                .AddTo(_lifetimeDisposables);

            _inputImage.OnDragAsObservable()
                .Subscribe(pointerEventData => {
                    _currentPointerPosition.Value = pointerEventData.position;
                    Vector2 newPointerPos = pointerEventData.position;
                    // horizontal swipes
                    float horizontalDistanceToSwipe = _horizontalSwipesCount == 0
                        ? Screen.width * (1 - _horizontalSwipeSensitivity)
                        : Screen.width * (1 - _secondHorizontalSwipeSensitivity);
                    if (Mathf.Abs(newPointerPos.x - _swipeStartPositionX) >= horizontalDistanceToSwipe) {
                        if (newPointerPos.x > _swipeStartPositionX) _contract.OnSwipeRight.Execute();
                        else _contract.OnSwipeLeft.Execute();
                        _horizontalSwipesCount++;
                        _swipeStartPositionX = newPointerPos.x;
                    }
                    // vertical swipes
                    float verticalDistanceToSwipe = _verticalSwipesCount == 0
                        ? Screen.width * (1 - _verticalSwipeSensitivity)
                        : _secondVerticalSwipeSensitivity != 0
                            ? Screen.width * (1 - _secondVerticalSwipeSensitivity)
                            : Screen.height;
                    if (Mathf.Abs(newPointerPos.y - _swipeStartPositionY) >= verticalDistanceToSwipe) {
                        if (newPointerPos.y > _swipeStartPositionY) _contract.OnSwipeUp.Execute();
                        else _contract.OnSwipeDown.Execute();
                        _verticalSwipesCount++;
                        _swipeStartPositionY = newPointerPos.y;
                    }
                })
                .AddTo(_lifetimeDisposables);

            _inputImage.OnPointerDownAsObservable()
                .Subscribe(pointerEventData => {
                    _currentPointerPosition.Value = pointerEventData.position;
                    _pointerPositionTimeAgo.Value = pointerEventData.position;
                    _horizontalSwipesCount = 0;
                    _verticalSwipesCount = 0;
                    _swipeStartPositionX = pointerEventData.position.x;
                    _swipeStartPositionY = pointerEventData.position.y;

                    // horizontal swipes
                    _absenceOfPointerHorizontalMovementDisposable.Clear();
                    Observable
                        .EveryUpdate()
                        .Delay(TimeSpan.FromSeconds(_pointerPositionTimeAgoTime))
                        .Subscribe(_ => {
                            if (Mathf.Abs(_currentPointerPosition.Value.x - _pointerPositionTimeAgo.Value.x) <
                                _swipeResetDistance) {
                                ResetHorizontalSwipe();
                            }
                        })
                        .AddTo(_absenceOfPointerHorizontalMovementDisposable);
                    // vertical swipes
                    _absenceOfPointerVerticalMovementDisposable.Clear();
                    Observable
                        .EveryUpdate()
                        .Delay(TimeSpan.FromSeconds(_pointerPositionTimeAgoTime))
                        .Subscribe(_ => {
                            if (Mathf.Abs(_currentPointerPosition.Value.y - _pointerPositionTimeAgo.Value.y) <
                                _swipeResetDistance) {
                                ResetVerticalSwipe();
                            }
                        })
                        .AddTo(_absenceOfPointerVerticalMovementDisposable);
                })
                .AddTo(_lifetimeDisposables);

            _inputImage.OnPointerUpAsObservable().Subscribe(pointerEventData => {
                    _currentPointerPosition.Value = pointerEventData.position;
                    _absenceOfPointerHorizontalMovementDisposable.Clear();
                    _absenceOfPointerVerticalMovementDisposable.Clear();
                })
                .AddTo(_lifetimeDisposables);
        }

        private void ResetHorizontalSwipe() {
            _horizontalSwipesCount = 0;
            _swipeStartPositionX = _currentPointerPosition.Value.x;
            _swipeHorizontalDirection = SwipeDirection.None;
        }

        private void ResetVerticalSwipe() {
            _verticalSwipesCount = 0;
            _swipeStartPositionY = _currentPointerPosition.Value.y;
            _swipeVerticalDirection = SwipeDirection.None;
        }
#endregion

#region Static joystick input logic
        private void BindJoystickInput() {
            float defaultPointsDensity = 1080 * 1920;
            float currentPointsDensity = Screen.width * Screen.height;
            _sJoystickMaxInputDistanceFromAnchor =
                _sJoystickMaxInputDistanceFromAnchor / defaultPointsDensity * currentPointsDensity;
            if (_sJoystickMaxInputDistanceFromAnchor == 0) _sJoystickMaxInputDistanceFromAnchor = 1;
            _sJoystickMinInputDistanceFromAnchor =
                _sJoystickMinInputDistanceFromAnchor / defaultPointsDensity * currentPointsDensity;

            Vector2 dragStartPosition = Vector2.zero;
            Vector2 currentPointerPos = Vector2.zero;
            _contract.StaticJoystickInputActive.Value = false;

            if (_usePresetAnchorPosition) {
                dragStartPosition = _presetAnchor.position;
                _contract.StaticJoystickInputStartPoint.Value = _presetAnchor.position;

                _inputImage.OnPointerDownAsObservable()
                    .Subscribe(pointerEventData => {
                        currentPointerPos = pointerEventData.position;
                        if (_sJoysticActiveAxes == ActiveAxes.OnlyX) {
                            currentPointerPos.y = dragStartPosition.y;
                        } else if (_sJoysticActiveAxes == ActiveAxes.OnlyY) {
                            currentPointerPos.x = dragStartPosition.x;
                        }
                        if (Vector2.Distance(dragStartPosition, currentPointerPos) < _sJoystickMinInputDistanceFromAnchor) {
                            _contract.CurrentStaticJoystickInput.Value = Vector2.zero;
                            _contract.StaticJoystickInputActive.Value = true;
                            return;
                        }
                        Vector2 inputOffset = currentPointerPos - dragStartPosition;
                        float inputStrength = (inputOffset.magnitude - _sJoystickMinInputDistanceFromAnchor) /
                                              (_sJoystickMaxInputDistanceFromAnchor - _sJoystickMinInputDistanceFromAnchor);
                        inputStrength = Mathf.Clamp(inputStrength, 0, 1);
                        Vector2 normalizedInput = inputOffset.normalized * inputStrength;
                        _contract.CurrentStaticJoystickInput.Value = normalizedInput;
                        _contract.StaticJoystickInputActive.Value = true;
                    })
                    .AddTo(_lifetimeDisposables);

                _inputImage.OnPointerUpAsObservable()
                    .Subscribe(pointerEventData => _contract.StaticJoystickInputActive.Value = false)
                    .AddTo(_lifetimeDisposables);
            } else {
                _inputImage.OnPointerDownAsObservable()
                    .Subscribe(pointerEventData => {
                        dragStartPosition = pointerEventData.position;
                        _contract.StaticJoystickInputStartPoint.Value = dragStartPosition;
                    })
                    .AddTo(_lifetimeDisposables);

                _inputImage.OnBeginDragAsObservable()
                    .Subscribe(pointerEventData => _contract.StaticJoystickInputActive.Value = true)
                    .AddTo(_lifetimeDisposables);

                _inputImage.OnEndDragAsObservable()
                    .Subscribe(pointerEventData => _contract.StaticJoystickInputActive.Value = false)
                    .AddTo(_lifetimeDisposables);
            }

            _inputImage.OnDragAsObservable()
                .Subscribe(pointerEventData => {
                    currentPointerPos = pointerEventData.position;
                    if (_sJoysticActiveAxes == ActiveAxes.OnlyX) {
                        currentPointerPos.y = dragStartPosition.y;
                    } else if (_sJoysticActiveAxes == ActiveAxes.OnlyY) {
                        currentPointerPos.x = dragStartPosition.x;
                    }
                    if (Vector2.Distance(dragStartPosition, currentPointerPos) < _sJoystickMinInputDistanceFromAnchor) {
                        _contract.CurrentStaticJoystickInput.Value = Vector2.zero;
                        return;
                    }
                    Vector2 inputOffset = currentPointerPos - dragStartPosition;
                    float inputStrength = (inputOffset.magnitude - _sJoystickMinInputDistanceFromAnchor) /
                                          (_sJoystickMaxInputDistanceFromAnchor - _sJoystickMinInputDistanceFromAnchor);
                    inputStrength = Mathf.Clamp(inputStrength, 0, 1);
                    Vector2 normalizedInput = inputOffset.normalized * inputStrength;
                    _contract.CurrentStaticJoystickInput.Value = normalizedInput;
                })
                .AddTo(_lifetimeDisposables);
        }
#endregion

#region Dynamic joystick input logic
        private void BindDragDirectionalInput() {
            float defaultPointsDensity = 1080 * 1920;
            float currentPointsDensity = Screen.width * Screen.height;
            _anchorPointMaxDistance = _anchorPointMaxDistance / defaultPointsDensity * currentPointsDensity;
            _anchorPointVelocity = _anchorPointVelocity / defaultPointsDensity * currentPointsDensity;
            _dJoystickMaxInputDistanceFromAnchor =
                _dJoystickMaxInputDistanceFromAnchor / defaultPointsDensity * currentPointsDensity;
            if (_dJoystickMaxInputDistanceFromAnchor == 0) _dJoystickMaxInputDistanceFromAnchor = 1;
            _dJoystickMinInputDistanceFromAnchor =
                _dJoystickMinInputDistanceFromAnchor / defaultPointsDensity * currentPointsDensity;

            Vector2 dragStartPosition = Vector2.zero;
            Vector2 currentPointerPos = Vector2.zero;
            _contract.DynamicJoystickInputActive.Value = false;

            _inputImage.OnPointerDownAsObservable()
                .Subscribe(pointerEventData => {
                    dragStartPosition = pointerEventData.position;
                    _contract.DynamicJoystickInputStartPoint.Value = dragStartPosition;
                })
                .AddTo(_lifetimeDisposables);

            _inputImage.OnDragAsObservable()
                .Subscribe(pointerEventData => {
                    currentPointerPos = pointerEventData.position;
                    if (_dJoysticActiveAxes == ActiveAxes.OnlyX) {
                        currentPointerPos.y = dragStartPosition.y;
                    } else if (_dJoysticActiveAxes == ActiveAxes.OnlyY) {
                        currentPointerPos.x = dragStartPosition.x;
                    }
                    if (!_useSmoothAnchorMovement) {
                        if (Vector2.Distance(dragStartPosition, currentPointerPos) > _anchorPointMaxDistance) {
                            Vector2 moveDir = dragStartPosition - currentPointerPos;
                            dragStartPosition = currentPointerPos + moveDir.normalized * _anchorPointMaxDistance / 2;
                        }
                    }
                    if (Vector2.Distance(dragStartPosition, currentPointerPos) < _dJoystickMinInputDistanceFromAnchor) {
                        _contract.DynamicJoystickInputStartPoint.Value = dragStartPosition;
                        _contract.CurrentDynamicJoystickInput.Value = Vector2.zero;
                        return;
                    }
                    Vector2 inputOffset = currentPointerPos - dragStartPosition;
                    float inputStrength = (inputOffset.magnitude - _dJoystickMinInputDistanceFromAnchor) /
                                          (_dJoystickMaxInputDistanceFromAnchor - _dJoystickMinInputDistanceFromAnchor);
                    inputStrength = Mathf.Clamp(inputStrength, 0, 1);
                    Vector2 normalizedInput = inputOffset.normalized * inputStrength;
                    _contract.DynamicJoystickInputStartPoint.Value = dragStartPosition;
                    _contract.CurrentDynamicJoystickInput.Value = normalizedInput;
                })
                .AddTo(_lifetimeDisposables);

            if (_useSmoothAnchorMovement) {
                Observable
                    .EveryUpdate()
                    .Where(_ => _contract.DynamicJoystickInputActive.Value)
                    .Subscribe(_ => {
                        if (Vector2.Distance(dragStartPosition, currentPointerPos) > _anchorPointMaxDistance) {
                            Vector2 moveDir = dragStartPosition - currentPointerPos;
                            dragStartPosition -= moveDir.normalized * _anchorPointVelocity;
                            _contract.DynamicJoystickInputStartPoint.Value = dragStartPosition;
                        }
                    })
                    .AddTo(_lifetimeDisposables);
            }

            _inputImage.OnEndDragAsObservable()
                .Subscribe(pointerEventData => _contract.CurrentDynamicJoystickInput.Value = Vector2.zero)
                .AddTo(_lifetimeDisposables);

            _inputImage.OnBeginDragAsObservable()
                .Subscribe(pointerEventData => _contract.DynamicJoystickInputActive.Value = true)
                .AddTo(_lifetimeDisposables);

            _inputImage.OnEndDragAsObservable()
                .Subscribe(pointerEventData => _contract.DynamicJoystickInputActive.Value = false)
                .AddTo(_lifetimeDisposables);
        }
#endregion

#region Pointer movement input logic
        private void BindPointerMovementInput() {
            Vector2 pointerPrevPos = Vector2.zero;
            int inputBufferTime = 10;
            _absenceOfPointerMovementDisposable.Clear();

            _inputImage.OnPointerDownAsObservable()
                .Subscribe(pointerEventData => pointerPrevPos = pointerEventData.position)
                .AddTo(_lifetimeDisposables);

            _inputImage.OnDragAsObservable()
                .Subscribe(pointerEventData => {
                    _contract.CurrentPointerMovement.Value = pointerEventData.position - pointerPrevPos;
                    pointerPrevPos = pointerEventData.position;

                    _absenceOfPointerMovementDisposable.Clear();
                    Observable
                        .Timer(TimeSpan.FromMilliseconds(inputBufferTime))
                        .Subscribe(_ => _contract.CurrentPointerMovement.Value = Vector2.zero)
                        .AddTo(_absenceOfPointerMovementDisposable);
                })
                .AddTo(_lifetimeDisposables);

            _inputImage.OnPointerUpAsObservable()
                .Subscribe(pointerEventData => _contract.CurrentPointerMovement.Value = Vector2.zero)
                .AddTo(_lifetimeDisposables);
        }
#endregion

#region Drag events logic
        private void BindOnDragEvents() {
            _inputImage.OnBeginDragAsObservable()
                .Subscribe(pointerEventData => _contract.OnDragBegin.Execute(pointerEventData.position))
                .AddTo(_lifetimeDisposables);

            _inputImage.OnDragAsObservable()
                .Subscribe(pointerEventData => _contract.OnDrag.Execute(pointerEventData.position))
                .AddTo(_lifetimeDisposables);

            _inputImage.OnEndDragAsObservable()
                .Subscribe(pointerEventData => _contract.OnDragEnd.Execute(pointerEventData.position))
                .AddTo(_lifetimeDisposables);
        }
#endregion


    }
}