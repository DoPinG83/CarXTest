using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Gui.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Core.Gui.Canvases;
using Core.Utils;
using DG.Tweening;
using Core.Common;
using Core.Common.Modules;
using UniRx;
using Core.Common.Extensions;
using PathologicalGames;

namespace Core.Gui
{

    public class CGUI : Contract<CGUI>
    {
        [Input] public readonly ReactiveProperty<bool> IsFadeEffectBlocking = new ReactiveProperty<bool>();

        [Input] public readonly ReactiveCommand<(float, Action)> DoFadeIn = new ReactiveCommand<(float, Action)>();
        [Input] public readonly ReactiveCommand<(float, Action)> DoFadeOut = new ReactiveCommand<(float, Action)>();

        [Output] public readonly ReactiveProperty<IGuiManager> GuiManager = new ReactiveProperty<IGuiManager>();
    }
    /// <summary>
    /// Реализация гуи менеджера под Core
    /// </summary>
    public class GuiManager : MonoBehaviour, IGuiManager
    {
        protected class DisplayVO
        {
            public GuiControllerBase Controller { get; internal set; }
            public DisplayMode DisplayMode { get; internal set; }
        }

        [SerializeField]
        StaticCanvas _staticCanvas;

        [SerializeField]
        DynamicCanvas _dynamicCanvas;

        protected Transform _root;
        protected GuiFactory _factory;

        protected LinkedList<DisplayVO> _guiStack = new LinkedList<DisplayVO>();
        protected GameObject _modal = null;
        private BackButtonListener _backButtonListener;

        protected Tweener _fadeTweener;

        protected SpawnPool _spawnPool;

        public event Action OnBackButton
        {
            add { _backButtonListener.OnBackButton += value; }
            remove { _backButtonListener.OnBackButton -= value; }
        }

        public event Action<GuiControllerBase> TopChanged;
        public event Action<GuiControllerBase, bool> OnGuiStackChanged;

        private readonly CompositeDisposable _lifetimeDisposables = new CompositeDisposable();

        private CGUI _contract;

        public string LastOpenedGui
        {
            get
            {
                if (OpenedGui != null && OpenedGui.Count > 0)
                    return OpenedGui[0];
                return string.Empty;
            }
        }
        public List<string> OpenedGui { get; private set; }

        private async void OnEnable()
        {
            _contract = this.Get<CGUI>();

            _spawnPool = PoolManager.Pools["GuiPool"];

            _root = _staticCanvas.transform;
            _factory = new GuiFactory();

            _backButtonListener = _root.GetComponentInChildren<BackButtonListener>();
            if (_backButtonListener == null)
            {
                var go = new GameObject();
                go.name = "BackButtonListener";
                go.transform.parent = _root;
                _backButtonListener = go.AddComponent<BackButtonListener>();
            }
            _backButtonListener.OnBackButton += OnAndroidBackButtonPressed;

#if UNITY_EDITOR
            DisableBackButtonProcessing(true, 0);
#endif

            _contract.DoFadeIn.Subscribe(DoFadeIn).AddTo(_lifetimeDisposables);
            _contract.DoFadeOut.Subscribe(DoFadeOut).AddTo(_lifetimeDisposables);

            _contract.GuiManager.Value = this;

            Modules.All.Register(_contract);
        }


        public event Action<bool> PresentationModeChanged;
        public bool IsPresentation {
            get; private set;
        }

        public void ImmediatelyCloseAllExcept(params Type[] except)
        {
            foreach (var vo in _guiStack)
            {
                var controller = vo.Controller as GuiController;
                if (controller != null)
                    controller.DisableAnimation();
            }
            CloseAllExcept(except);
        }

        protected virtual void SetEnvironment(GuiControllerBase controller)
        {
            GuiController guiController = controller as GuiController;
            if (guiController != null)
                guiController.SetEnvironment(this);
        }

        public T Show<T>(DisplayMode mode, Action<T> initMethod) where T : GuiControllerBase
        {
            var uiName = typeof(T).Name;
            CoreLog.Log(string.Format("Show GUI: {0} with mode {1}", uiName, mode));

            var controller = UiShow<T>(mode);
            initMethod?.Invoke(controller);

            controller.transform.localRotation = Quaternion.identity;

            if (mode == DisplayMode.DialogCloseOthers && (controller as GuiController) != null)
            {
                (controller as GuiController).ResetSortingOrder();
                UpdateModal();
            }

            if (OpenedGui == null)
                OpenedGui = new List<string>();
            OpenedGui.Insert(0, controller.GetType().Name);
            if (OpenedGui.Count > 10)
                OpenedGui.RemoveAt(OpenedGui.Count - 1);

            OnGuiStackChanged?.Invoke(controller, true);
            return controller;
        }

        public T Show<T>(DisplayMode mode = DisplayMode.DialogAddFront) where T : GuiControllerBase
        {
            return Show<T>(mode, null);
        }

        public virtual T TryGetOrCreate<T>(DisplayMode mode = DisplayMode.DialogAddFront) where T : GuiControllerBase
        {
            return TryGet<T>() ?? Show<T>(mode);
        }

        public virtual T TryGetOrCreate<T>(DisplayMode mode, Action<T> initMethod) where T : GuiControllerBase
        {
            return TryGet<T>() ?? Show(mode, initMethod);
        }

        protected virtual void InitModal(GuiControllerBase dialog)
        {
            _modal.transform.localRotation = Quaternion.identity;
            Modal modal = _modal.GetComponent<Modal>();
            modal.Init(this, dialog);
        }

        protected virtual void UpdateModal()
        {
            if (_modal == null)
                return;

            _modal.transform.SetAsLastSibling();
            int lastDialogIndex = 0;
            GuiControllerBase lastDialog = null;
            foreach (var guiVO in _guiStack)
            {
                if (!IsDialog(guiVO.DisplayMode))
                    continue;

                int idx = guiVO.Controller.transform.GetSiblingIndex();
                if (idx > lastDialogIndex)
                {
                    lastDialog = guiVO.Controller;
                    lastDialogIndex = idx;
                }
            }

            int index = Mathf.Max(lastDialogIndex, 0);
            _modal.transform.SetSiblingIndex(index);
            InitModal(lastDialog);

            if (_modal == null)
                return;

            DisplayVO displayVo = _guiStack.LastOrDefault(vo =>
            {
                GuiController controller = vo.Controller as GuiController;
                if (controller != null) return controller.InOwnCanvas();
                return false;
            });
            if (displayVo != null)
            {
                Canvas dialogCanvas = displayVo.Controller.GetComponent<Canvas>();
                if (dialogCanvas != null)
                {
                    Canvas canvas = _modal.GetComponent<Canvas>();
                    if (canvas == null)
                    {
                        canvas = _modal.AddComponent<Canvas>();
                        _modal.AddComponent<GraphicRaycaster>();
                    }
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = dialogCanvas.sortingOrder - 1;
                }
            }
        }

        /// <summary>
        /// Создает и показывает диалог
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected virtual T UiShow<T>(DisplayMode mode = DisplayMode.DialogAddFront) where T : GuiControllerBase
        {
            return (T)Show(typeof(T), mode);
        }

        public virtual void ShowAlert(string message)
        {
            GetScreenMessage(s => s.ShowAlert(message));
        }

        public virtual void ShowWarning(string message, float delay = -1)
        {
            GetScreenMessage(s => s.ShowWarning(message, delay));
        }

        public virtual void ShowMessage(string message)
        {
            GetScreenMessage(s => s.ShowMessage(message));
        }

        public virtual void ShowConnection(bool connecting, string message = "")
        {
            GetScreenMessage(s => s.ShowConnection(connecting, message));
        }

        private void GetScreenMessage(Action<ScreenMessage> cb)
        {
            var screen = TryGetOrCreate<ScreenMessage>(DisplayMode.Persistent);
            if (cb != null && screen != null)
                cb(screen);
        }

        protected virtual void OnAndroidBackButtonPressedAndNobodyCares()
        {
            //Show conformation dialog for quit the app
        }

        private int _priorityOfDisableBackButtonProcessing = 0;
        public void DisableBackButtonProcessing(bool value, int priority)   // priority реализует перехват управления функцией DisableBackButtonProcessing (для тутора)
        {
            if (priority >= _priorityOfDisableBackButtonProcessing)
            {
                DisableBackButtonProcessing(value);
                _priorityOfDisableBackButtonProcessing = priority;
            }
        }

        public void Close<T>() where T : GuiControllerBase
        {
            CloseInStack(vo => vo.Controller is T);
        }

        public bool HasOpenWindows()
        {
            foreach (DisplayVO displayVo in _guiStack)
            {
                if (displayVo.Controller is GuiController)
                {
                    if (((GuiController)(displayVo.Controller)).InOwnCanvas())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ModalWindowOpened()
        {
            return _guiStack.Any(dvp => IsDialog(dvp.DisplayMode));
        }

        public int GetCurrentMaxSortingOrder()
        {
            int i = 0;
            foreach (DisplayVO displayVo in _guiStack)
            {
                if (displayVo.Controller is GuiController)
                    i = Math.Max(i, ((GuiController)(displayVo.Controller)).SortingOrder);
            }
            return i;
        }

        public void OnAndroidBackButtonPressed()
        {
            for (var it = _guiStack.Last; it != null; it = it.Previous)
            {
                if (it.Value.Controller.OnAndroidBackButton())
                {
                    return;
                }
            }

            OnAndroidBackButtonPressedAndNobodyCares();
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            var controllers = new List<T>();
            foreach (var vo in _guiStack)
            {
                var tmp = vo.Controller as T;
                if (tmp != null)
                    controllers.Add(tmp);
            }
            return controllers;
        }

        /// <summary>
        /// Возвращает диалог из стека, если он есть
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mode"></param>
        /// <returns></returns>
        public virtual T TryGet<T>() where T : GuiControllerBase
        {
            return GetFromStack<T>();
        }

        /// <summary>
        /// Если указанный в параметре типа диалог есть в стеке - вызывает коллбек, передавая объект этого диалога
        /// </summary>
        public virtual void TryGet<T>(Action<T> callback, bool waitForController = false) where T : GuiControllerBase
        {
            if (waitForController)
                WaitForController(callback).Run();
            else
            {
                var controller = GetFromStack<T>();
                if (controller != null)
                    callback(controller);
            }
        }

        /// <summary>
        /// Возвращает диалог из стека, если он есть
        /// </summary>
        /// <param name="typeToGet"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public virtual GuiControllerBase TryGet(Type typeToGet)
        {
            return GetFromStack(typeToGet);
        }

        /// <summary>
        /// Создает и показывает диалог
        /// </summary>
        /// <param name="guiType"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public virtual GuiControllerBase Show(Type guiType, DisplayMode mode = DisplayMode.DialogAddFront)
        {
            // если показываем в режиме одного скрина, закрываем имеющийся
            if (mode == DisplayMode.Screen)
            {
                CloseInStack(vo => IsScreen(vo.DisplayMode));
            }
            else if (mode == DisplayMode.DialogHideScreen)
                SetHide(vo => IsScreen(vo.DisplayMode), true);

            // создаем объект сцены и добавляем в массив
            GameObject obj = _factory.CreateGuiObject(guiType);
            GuiControllerBase controller = obj.GetComponent(guiType) as GuiControllerBase;
            controller.AnimateOpen();

            InitController(obj, controller, mode);

            return controller;
        }

        private void InitController(GameObject guiObject, GuiControllerBase controller, DisplayMode mode)
        {
            SetEnvironment(controller);
            ResetRectTransform(guiObject.transform);

            DisplayVO state = new DisplayVO
            {
                Controller = controller,
                DisplayMode = mode
            };
            controller.Setup(mode);

            DisplayVO vo = null;
            if (_guiStack.Count > 0)
                vo = _guiStack.FirstOrDefault(gui => gui.DisplayMode == DisplayMode.Persistent);

            if (IsDialog(mode) && mode != DisplayMode.Dialog)
            {
                foreach (var vo2 in _guiStack.Where(displayVO => IsDialog(displayVO.DisplayMode)))
                    vo2.Controller.Hide(true);
            }

            if (mode == DisplayMode.Screen && vo != null)
            {
                _guiStack.AddBefore(_guiStack.Find(vo), state);
                controller.gameObject.transform.SetSiblingIndex(vo.Controller.gameObject.transform.GetSiblingIndex());
            }
            else
            {
                _guiStack.AddLast(state);
                OnTopChanged();
            }

            if (IsDialog(mode))
            {
                CreateModal();
                UpdateModal();
            }

            // если в режиме закрывания других диалогов, то закрываем все другие диалоги
            if (mode == DisplayMode.DialogCloseOthers)
                CloseInStack(vo3 => IsDialog(vo3.DisplayMode) && vo3.Controller != controller);
        }

        protected void CloseInStack(Func<DisplayVO, bool> predicate)
        {
            IEnumerable<DisplayVO> screens =
                new List<DisplayVO>(_guiStack.Where(predicate));
            foreach (DisplayVO screen in screens)
                Close(screen.Controller);
        }

        protected void SetHide(Func<DisplayVO, bool> predicate, bool value)
        {
            IEnumerable<DisplayVO> screens =
                new List<DisplayVO>(_guiStack.Where(predicate));
            foreach (DisplayVO screen in screens)
                screen.Controller.Hide(value);
        }

        private GuiControllerBase GetFromStack(Type t)
        {
            foreach (var vo in _guiStack)
            {
                var type = vo.Controller.GetType();
                if (type == t || type.IsSubclassOf(t) || type.GetInterfaces().Contains(t))
                    return vo.Controller;
            }
            return null;
        }

        private T GetFromStack<T>() where T : GuiControllerBase
        {
            foreach (var vo in _guiStack)
            {
                var tmp = vo.Controller as T;
                if (tmp != null)
                {
                    return tmp;
                }
            }
            return null;
        }

        private bool IsDialog(DisplayMode mode)
        {
            return mode == DisplayMode.DialogAddFront ||
                   mode == DisplayMode.DialogHideScreen ||
                   mode == DisplayMode.DialogCloseOthers || 
                   mode == DisplayMode.Dialog;
        }

        private bool IsScreen(DisplayMode mode)
        {
            return mode == DisplayMode.Screen;
        }

        private void OnTopChanged()
        {
            if (TopChanged != null)
                TopChanged(GetStackLast());
        }

        private void CreateModal()
        {
            if (_modal != null)
                return;
            _modal = _factory.CreateModalObject();
            if (_modal != null)
                ResetRectTransform(_modal.transform);
        }

        /// <summary>
        /// Закрывает выбранное окно
        /// </summary>
        /// <param name="controller"></param>
        public virtual void Close(GuiControllerBase controller, bool animate = true)
        {
            CoreLog.Log(string.Format("Close GUI: {0}", controller.GetType().Name));
            DisplayVO vo = _guiStack.FirstOrDefault(state => state.Controller == controller);
            if (vo == null)
            {
                CoreLog.LogWarning(string.Format("Cannot close screen {0}. It's not in the GUI stack", controller.GetType()));
                return;
            }

            // Убираем из списка диалог и уничтожаем объект
            _guiStack.Remove(vo);

            Action onDespawn = () =>
            {
                // если мы закрыли диалог, мы должны показать диалог, который был под ним
                if (IsDialog(vo.DisplayMode))
                {
                    var topVO = _guiStack.LastOrDefault(vo2 => IsDialog(vo2.DisplayMode));
                    if (topVO != null && topVO.Controller.IsHidden)
                    {
                        topVO.Controller.Hide(false);
                        topVO.Controller.AnimateOpen();
                    }
                }

                OnTopChanged();

                // Обновляем или уничтожаем модальный блокер
                if (IsDialog(vo.DisplayMode))
                {
                    // Если нет больше диалогов в стеке
                    if (_guiStack.Count(displayVO => IsDialog(displayVO.DisplayMode)) <= 0)
                    {
                        DespawnModal();
                        //Показываем скрытый скрин
                        var screen = _guiStack.LastOrDefault(vo2 => IsScreen(vo2.DisplayMode));
                        if (screen != null && screen.Controller.IsHidden)
                        {
                            screen.Controller.Hide(false);
                            screen.Controller.AnimateOpen();
                        }
                    }
                    else
                        UpdateModal();
                }
                if (OnGuiStackChanged != null)
                    OnGuiStackChanged(controller, false);
            };

            if (animate)
                AnimateAndDespawn(vo.Controller, onDespawn);
            else
            {
                Despawn(vo.Controller);
                onDespawn();
            }
        }

        public GuiControllerBase GetStackLast()
        {
            return _guiStack.Last != null ? _guiStack.Last.Value.Controller : null;
        }

        public GuiControllerBase GetStackLast(DisplayMode mode)
        {
            var last = _guiStack.Last;
            while (last != null)
            {
                if (last.Value.DisplayMode == mode)
                    return last.Value.Controller;

                last = last.Previous;
            }

            return null;
        }

        public List<GuiControllerBase> GetStack(DisplayMode mode)
        {
            var result = new List<GuiControllerBase>();
            foreach (var screen in _guiStack)
            {
                if(screen.DisplayMode == mode)
                    result.Add(screen.Controller);
            }

            return result;
        }

        /// <summary>
        /// Закрывает все объекты ГУИ в руте
        /// </summary>
        public virtual void CloseAll()
        {
            DespawnModal();
            foreach (var vo in _guiStack)
                AnimateAndDespawn(vo.Controller);
            _guiStack.Clear();
        }

        public virtual void CloseAll(Func<GuiControllerBase, bool> predicate)
        {
            DespawnModal();
            var newStack = new LinkedList<DisplayVO>();
            foreach (var vo in _guiStack)
            {
                if (predicate(vo.Controller))
                    AnimateAndDespawn(vo.Controller);
                else
                    newStack.AddLast(vo);
            }

            _guiStack = newStack;
        }

        /// <summary>
        /// Закрывает все объекты ГУИ в руте
        /// </summary>
        /// <param name="excepts">Исключения, которые не следую закрывать</param>
        public virtual void CloseAllExcept(params Type[] excepts)
        {
            if (!excepts.Contains(typeof(Modal)))
                DespawnModal();

            DisplayVO[] copy = new DisplayVO[_guiStack.Count];
            _guiStack.CopyTo(copy, 0);

            foreach (var vo in copy)
            {
                var voType = vo.Controller.GetType();
                if (excepts == null || !excepts.Contains(voType))
                {
                    AnimateAndDespawn(vo.Controller);
                    _guiStack.Remove(vo);
                    OnGuiStackChanged?.Invoke(vo.Controller, false);
                }                
            }

            OnTopChanged();
        }

        private void DespawnModal()
        {
            if (_modal != null)
            {
                _modal.transform.SetParent(_spawnPool.transform);
                _spawnPool.Despawn(_modal.transform);
            }
            _modal = null;
        }

        private void AnimateAndDespawn(GuiControllerBase controller, Action callback = null)
        {
            controller.OnClose = () =>
            {
                Despawn(controller);
                if (callback != null)
                    callback();
            };
            controller.AnimateClose();
        }

        private void Despawn(GuiControllerBase controller)
        {
            controller.Dispose();

            if (controller.DontDestroyWhenClose)
            {
                controller.gameObject.SetActive(false);
            }
            else
            {
                controller.gameObject.transform.SetParent(_spawnPool.transform);
                _spawnPool.Despawn(controller.gameObject.transform);
            }
        }

        private void ResetRectTransform(Transform transform)
        {
            RectTransform rect = (RectTransform)transform;
            rect.SetParent(_root);
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
        }

        public void CloseAllDialogs()
        {
            CloseInStack(vo => IsDialog(vo.DisplayMode));
        }

        public void DisableBackButtonProcessing(bool disable)
        {
            _backButtonListener.DisableProcessing = disable;
        }

        public virtual void Dispose()
        {
            _lifetimeDisposables.Clear();
            _backButtonListener.OnBackButton -= OnAndroidBackButtonPressed;
        }

        private IEnumerator WaitForController<T>(Action<T> onLoad) where T : GuiControllerBase
        {
            var controller = GetFromStack<T>();
            while (controller == null)
            {
                yield return null;
                controller = GetFromStack<T>();
            }
            onLoad(controller);
        }

        private void DoFadeOut((float, Action) fadeInfo)
        {
            if (_staticCanvas.FadeCanvasGroup == null)
            {
                Debug.LogError("No fade canvas group attached");
                return;
            }
            _fadeTweener?.Kill();
            _staticCanvas.FadeCanvasGroup
                .DOFade(0, fadeInfo.Item1).SetUpdate(true)
                .OnComplete(() =>
                {
                    _staticCanvas.FadeCanvasGroup.blocksRaycasts = false;
                    fadeInfo.Item2?.Invoke();
                });
        }

        private void DoFadeIn((float, Action) fadeInfo)
        {
            if (_staticCanvas.FadeCanvasGroup == null)
            {
                Debug.LogError("No fade canvas group attached");
                return;
            }

            _staticCanvas.FadeCanvasGroup.transform.SetAsLastSibling();

            if (_contract.IsFadeEffectBlocking.Value)
                _staticCanvas.FadeCanvasGroup.blocksRaycasts = true;

            _fadeTweener?.Kill();
            _staticCanvas.FadeCanvasGroup
                .DOFade(1, fadeInfo.Item1).SetUpdate(true).OnComplete(() => fadeInfo.Item2?.Invoke());
        }

        private void OnDisable()
        {
            _lifetimeDisposables.Clear();
        }
    }
}