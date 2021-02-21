using Core.Gui;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Gui
{
    public class Modal : MonoBehaviour
    {
        private GuiManager _guiManager;
        private GuiControllerBase _dialogController;

        public void Init(GuiManager guiManager, GuiControllerBase dialogController)
        {
            _guiManager = guiManager;
            _dialogController = dialogController;
        }

        public void OnClicked()
        {
            // TODO: чтобы убрать этот хак, нужно сделать так чтобы GuiManager работал с GuiController,
            // но для этого нужно убрать хаки из него :)
            var modalAwareController = _dialogController as GuiController;
            if (modalAwareController != null)
            {
                if (!modalAwareController.IsModal)
                    modalAwareController.Close();
            }
            else
            {
                _guiManager.Close(_dialogController);
            }
        }
    }
}
