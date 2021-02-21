using UnityEngine;
using UnityEngine.UI;

namespace Core.Gui.Controllers.Elements
{
    public class ConnectionElement : MonoBehaviour
    {
        [SerializeField] private Text _label;
        [SerializeField] private ConnectingIcon _connectingIcon;

        public void Show(bool show, string text)
        {
            _label.text = text;

            if (show && gameObject.activeInHierarchy ||
                !show && !gameObject.activeInHierarchy)
                return;

            _connectingIcon.Show(show);

            gameObject.SetActive(show);
        }
    }
}