namespace Core.Common.InfoSystems.SystemImplementation {
    using System;
    using System.Collections.Generic;
    using UniRx;
    using Debug = UnityEngine.Debug;

    public class Infos : Singleton<Infos> {

        private bool _logDebug = false;

        public readonly ReactiveCommand<(Type, object)> NewInfoAdded = new ReactiveCommand<(Type, object)>();
        public readonly ReactiveCommand<(Type, object)> InfoRemoved  = new ReactiveCommand<(Type, object)>();

        private Dictionary<Type, object> _infos = new Dictionary<Type, object>();

        public void AddInfo<T>(T newInfo, ref LinkedListNode<T> listNode) {
            Type infoType = typeof(T);
            if (!_infos.ContainsKey(infoType)) {
                _infos.Add(infoType, new LinkedList<T>());
            }
            LinkedList<T> addedInfos = _infos[infoType] as LinkedList<T>;
            addedInfos.AddLast(newInfo);
            listNode = addedInfos.Last;
            NewInfoAdded.Execute((infoType, newInfo));
            if (_logDebug) Debug.Log($"[Infos]:\tAdded info of type #{infoType}#");
        }

        public void RemoveInfo<T>(T info, LinkedListNode<T> listNode) {
            Type infoType = typeof(T);
            if (!_infos.ContainsKey(infoType)) return;
            LinkedList<T> addedInfos = _infos[infoType] as LinkedList<T>;
            addedInfos.Remove(listNode);
            InfoRemoved.Execute((infoType, info));
            if (_logDebug) Debug.Log($"[Infos]:\tRemoved info of type #{infoType}#");
        }

        public object GetInfosOfType(Type infoType) {
            if (!_infos.ContainsKey(infoType)) {
                Type listType = typeof(LinkedList<>);
                Type constructedListType = listType.MakeGenericType(infoType);
                object listInstance = Activator.CreateInstance(constructedListType);
                _infos.Add(infoType, listInstance);
                return _infos[infoType];
            }
            return _infos[infoType];
        }

        protected Infos() { }

    }
}