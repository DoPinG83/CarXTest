using System;
using Core.Gui.Controllers.Elements;
using UnityEngine;

namespace Core.Gui.Data
{
    [Serializable]
    public class TabCallbackEntry
    {
        public DialogTab Tab;
        public Action Callback { get { return _callback.Action; }}

        [SerializeField]
        private ActionEntry _callback;

        public void Awake()
        {
            _callback.Awake();
        }
    }
}
