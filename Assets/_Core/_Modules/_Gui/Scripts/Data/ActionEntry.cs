using System;
using System.Reflection;
using UnityEngine;

namespace Core.Gui.Data
{
    [Serializable]
    public class ActionEntry
    {
        [SerializeField]
        private UnityEngine.Object _target;

        [SerializeField]
        private string _method;

        [SerializeField]
        [HideInInspector]
        private string[] _candidates;

        [SerializeField]
        [HideInInspector]
        private UnityEngine.Object _component;

        private Action _action;

        public Action Action { get { return _action; } }

        public void Awake()
        {
            _action = Action.CreateDelegate(typeof(Action), _component ?? _target, (_component ?? _target).GetType().GetMethod(_method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) as Action;
        }
    }
}
