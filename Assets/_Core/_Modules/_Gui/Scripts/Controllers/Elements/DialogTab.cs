
using Core.Utils.GuiEffect;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Gui.Controllers.Elements
{
    public class DialogTab : MonoBehaviour
    {
        [SerializeField] protected string _id;
        [SerializeField] protected Button _button;
        [SerializeField] protected GameObject _selectedView;
        [SerializeField] protected GameObject _deselectedView;

        protected bool _selected = false;
        protected bool _enabled = true;

        public Button Button 
        {
            get { return _button;  }
        }

        public string Id
        {
            get { return _id; }
        }

        public virtual bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                _selectedView.SetActive(_selected);
                _deselectedView.SetActive(!_selected);
            }
        }

        public virtual bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (!_enabled)
                {
                    Selected = false;
                    GuiEffect.Desaturate(_deselectedView, this, true);
                }
                else
                {
                    GuiEffect.Reset(_deselectedView, this);
                }

                _button.interactable = _enabled;
            }
        }
    }
}
