using System;
using System.Collections.Generic;

namespace Core.Gui
{
    public interface IGuiManager: IDisposable
    {
        event Action OnBackButton;
        event Action<GuiControllerBase> TopChanged;
        event Action<GuiControllerBase, bool> OnGuiStackChanged;
        void OnAndroidBackButtonPressed();
        void DisableBackButtonProcessing(bool value, int priority);
        void DisableBackButtonProcessing(bool disable);

        string LastOpenedGui { get; }
        List<string> OpenedGui { get; }

        IEnumerable<T> GetAll<T>() where T : class;
        T TryGet<T>() where T : GuiControllerBase;
        void TryGet<T>(Action<T> callback, bool waitForController) where T : GuiControllerBase;
        T TryGetOrCreate<T>(DisplayMode mode = DisplayMode.DialogAddFront) where T : GuiControllerBase;
        T TryGetOrCreate<T>(DisplayMode mode, Action<T> initMethod) where T : GuiControllerBase;
        GuiControllerBase TryGet(Type typeToGet);
        T Show<T>(DisplayMode mode = DisplayMode.DialogAddFront) where T : GuiControllerBase;
        T Show<T>(DisplayMode mode, Action<T> initMethod) where T : GuiControllerBase;
        GuiControllerBase Show(Type guiType, DisplayMode mode = DisplayMode.DialogAddFront);
        GuiControllerBase GetStackLast();
        GuiControllerBase GetStackLast(DisplayMode mode);
        List<GuiControllerBase> GetStack(DisplayMode mode);

        bool HasOpenWindows();
        bool ModalWindowOpened();

        int GetCurrentMaxSortingOrder();

        void ShowAlert(string message);
        void ShowWarning(string message, float delay = -1);
        void ShowMessage(string message);
        void ShowConnection(bool connecting, string message = "");

        void Close(GuiControllerBase controller, bool animate = true);
        void Close<T>() where T : GuiControllerBase;
        void CloseAll();
        void CloseAll(Func<GuiControllerBase, bool> predicate);
        void ImmediatelyCloseAllExcept(params Type[] except);
        void CloseAllExcept(params Type[] except);
        void CloseAllDialogs();
    }
}
