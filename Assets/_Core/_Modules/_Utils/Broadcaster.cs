/// Do not delete this, uncomment following line to debug event dublication;
//#define DEBUG_EVENTS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Utils
{
    public struct EventParams : IEnumerable<object>
    {
        public object this[int index] { get { return m_Args[index]; } }

        readonly object[] m_Args;
        uint m_CurrentPosition;

        public EventParams(params object[] args)
        {
            m_Args = args;
            m_CurrentPosition = 0;
        }

        public static explicit operator object[](EventParams obj)
        {
            return obj.m_Args;
        }

        public T Next<T>()
        {
            var element = ElementAt<T>(m_CurrentPosition);
            m_CurrentPosition++;
            return element;
        }

        public T ElementAt<T>(uint index)
        {
            if (index >= m_Args.Length)
            {
                CoreLog.LogErrorFormat("Index is {0} but args count is {1}", index, m_Args.Length);
                return default;
            }

            var current = m_Args[m_CurrentPosition];
            var casted = current is T ? (T)current : default;
            return casted;
        }

        public T FirstOf<T>()
        {
            return m_Args
                .OfType<T>()
                .FirstOrDefault();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)m_Args).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<object>)m_Args).GetEnumerator();
        }

        public static readonly EventParams Empty = new EventParams(new object[] { });
    }

    public interface IBroadcaster
    {
        void SetListener(bool isSubscribe, int eventCode, Action<EventParams> listener);

        void AddListener(int eventCode, Action<EventParams> listener);
        void AddListener(GameObject gameObject, int eventCode, Action<EventParams> listener);

        void RemoveListener(int eventCode, Action<EventParams> listener);
        void RemoveListener(GameObject gameObject, int eventCode, Action<EventParams> listener);

        void Invoke(int eventCode, EventParams args);
        void Invoke(GameObject gameObject, int eventCode, bool requireReceiver, EventParams args);
        void Invoke(Component component, int eventCode, bool requireReceiver, EventParams args);

        void ClearAndRemoveAllListeners();

        void PrintListenersFor(int eventCode);
    }

    public class Broadcaster : IBroadcaster
    {
        #region Singleton
        static Broadcaster _instance;

        public static Broadcaster Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Broadcaster();

                return _instance;
            }
        }
        #endregion

        Broadcaster()
        {
            m_DefaultEventCollection = new InternalEventCollection();
            m_GameObjectEventCollection = new Dictionary<int, InternalEventCollection>();
        }

        #region INTERNAL
        class InternalEventCollection : IEnumerable<KeyValuePair<int, InternalEvent>>
        {
            Dictionary<int, InternalEvent> m_Events;

            public InternalEventCollection()
            {
                m_Events = new Dictionary<int, InternalEvent>();
            }

            public void AddListener(int eventCode, Action<EventParams> listener)
            {
                InternalEvent targetEvent = null;
                if (m_Events.TryGetValue(eventCode, out targetEvent))
                    targetEvent.AddListener(listener);
                else
                    m_Events.Add(eventCode, new InternalEvent(listener));
            }

            public bool Invoke(int eventCode, EventParams args)
            {
                InternalEvent targetEvent = null;
                if (m_Events.TryGetValue(eventCode, out targetEvent))
                    targetEvent.Invoke(args);

                return targetEvent != null;
            }

            public void RemoveListener(int eventCode, Action<EventParams> listener)
            {
                InternalEvent targetEvent = null;
                if (m_Events.TryGetValue(eventCode, out targetEvent))
                {
                    targetEvent.RemoveListener(listener);
                    if (targetEvent.IsEmpty)
                        m_Events.Remove(eventCode);
                }
            }

            public void PrintListenersFor(int eventCode)
            {
                if (m_Events.ContainsKey(eventCode))
                    m_Events[eventCode].PrintListeners();
                else
                    CoreLog.LogErrorFormat("No listeners for event with code {0}", eventCode);
            }

            public void Clear()
            {
                foreach (var e in m_Events.Values)
                    e.RemoveAllListeners();
                m_Events.Clear();
            }

            public IEnumerator<KeyValuePair<int, InternalEvent>> GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<int, InternalEvent>>)m_Events).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<int, InternalEvent>>)m_Events).GetEnumerator();
            }
        }

        class InternalEvent
        {
            public bool IsEmpty { get { return ListenerCount == 0; } }
            public int ListenerCount { get { return m_Event.GetInvocationList().Length; } }

            event Action<EventParams> m_Event;

            public InternalEvent()
            {
                m_Event = delegate { };
            }

            public InternalEvent(Action<EventParams> listener) : this()
            {
                AddListener(listener);
            }

            public void AddListener(Action<EventParams> callback)
            {
#if UNITY_EDITOR && DEBUG_EVENTS
                CheckListenerDublication(callback, (callbackName, sameListeners) => 
                {
                    if (sameListeners.Any())
                        CoreLog.LogErrorFormat("Event already has {0} listener(s) with name {1}. This can lead to unexpected behaviour. Maybe we need remove listeners?", sameListeners.Count(), callbackName);
                });
#endif
                m_Event += callback;
            }

            public void RemoveListener(Action<EventParams> callback)
            {
#if UNITY_EDITOR && DEBUG_EVENTS
                CheckListenerDublication(callback, (callbackName, sameListeners) =>
                {
                    if (!sameListeners.Any())
                        CoreLog.LogErrorFormat("Event doesn't have any listeners with name {0}. Looks like nobody subsribes it", callbackName);
                });
#endif
                m_Event -= callback;
            }

            public void RemoveAllListeners()
            {
                m_Event = delegate { };
            }

            public void Invoke(EventParams args)
            {
                m_Event(args);
            }

            public void PrintListeners()
            {
                foreach (var m in m_Event.GetInvocationList())
                    CoreLog.LogError(MethodInfoToString(m.Method));
            }

            string MethodInfoToString(System.Reflection.MethodInfo info)
            {
                return string.Format("{0}.{1}", info.DeclaringType.FullName, info.Name);
            }

#if UNITY_EDITOR
            void CheckListenerDublication(Action<EventParams> listener, Action<string, IEnumerable<Delegate>> resultProcessor)
            {
                if (listener == null)
                    throw new ArgumentNullException("listener");

                // Small debug functionality;
                var listenerMethodName = MethodInfoToString(listener.Method);
                var sameListeners = m_Event
                    .GetInvocationList()
                    .Where(d => MethodInfoToString(d.Method).Equals(listenerMethodName));

                resultProcessor(listenerMethodName, sameListeners);
            }
#endif
        }
        #endregion

        /// <summary>
        /// Global events without reference to game objects;
        /// </summary>
        InternalEventCollection m_DefaultEventCollection;

        /// <summary>
        /// Events for game objects;
        /// Key is a game object's instance id here;
        /// </summary>
        Dictionary<int, InternalEventCollection> m_GameObjectEventCollection;

        public void SetListener(bool isSubscribe, int eventCode, Action<EventParams> listener)
        {
            if (isSubscribe)
                AddListener(eventCode, listener);
            else
                RemoveListener(eventCode, listener);
        }

        /// <summary>
        /// Add listener for global event;
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="listener"></param>
        public void AddListener(int eventCode, Action<EventParams> listener)
        {
            m_DefaultEventCollection.AddListener(eventCode, listener);
        }

        /// <summary>
        /// Add listener for event and attach it to specific gmae object;
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="eventCode"></param>
        /// <param name="listener"></param>
        public void AddListener(GameObject gameObject, int eventCode, Action<EventParams> listener)
        {
            if (gameObject == null)
            {
                CoreLog.LogError("Trying to add listener, but target game object in null");
                return;
            }
            var instanceId = gameObject.GetInstanceID();

            // Already has instance;
            if (m_GameObjectEventCollection.ContainsKey(instanceId))
                m_GameObjectEventCollection[instanceId].AddListener(eventCode, listener);
            //Create new collection for game object;
            else
            {
                var c = new InternalEventCollection();
                c.AddListener(eventCode, listener);
                m_GameObjectEventCollection.Add(instanceId, c);
            }
        }

        /// <summary>
        /// Removes all listeners and all events;
        /// </summary>
        public void ClearAndRemoveAllListeners()
        {
            m_DefaultEventCollection.Clear();
            foreach (var c in m_GameObjectEventCollection.Values)
                c.Clear();

            m_GameObjectEventCollection.Clear();
        }

        /// <summary>
        /// Invoke global event with event code;
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="args"></param>
        public void Invoke(int eventCode, EventParams args)
        {
            m_DefaultEventCollection.Invoke(eventCode, args);
        }

        /// <summary>
        /// Convenience method
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventCode"></param>
        /// <param name="requireReceiver"></param>
        /// <param name="args"></param>
        public void Invoke(Component component, int eventCode, bool requireReceiver, EventParams args)
        {
            if (component == null)
            {
                CoreLog.LogError("Target component is null");
                return;
            }
            Invoke(component.gameObject, eventCode, requireReceiver, args);
        }

        /// <summary>
        /// Invoke event that attached to specific game object;
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="eventCode"></param>
        /// <param name="requireReceiver"></param>
        /// <param name="args"></param>
        public void Invoke(GameObject gameObject, int eventCode, bool requireReceiver, EventParams args)
        {
            if (gameObject == null)
            {
                CoreLog.LogError("Target game object is null");
                return;
            }
            var instanceId = gameObject.GetInstanceID();

            if (m_GameObjectEventCollection.ContainsKey(instanceId))
            {
                if (!m_GameObjectEventCollection[instanceId].Invoke(eventCode, args) && requireReceiver)
                {
                    CoreLog.LogErrorFormat("Unable to find any event with code {0} that is registered for game object {1}", eventCode, gameObject.name);
                    return;
                }

            }
            else if (requireReceiver)
                CoreLog.LogErrorFormat("Target game object {0} doesn't registered", gameObject.name);
        }

        /// <summary>
        /// Remove listener for global event;
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="listener"></param>
        public void RemoveListener(int eventCode, Action<EventParams> listener)
        {
            m_DefaultEventCollection.RemoveListener(eventCode, listener);
        }

        /// <summary>
        /// Remove lister for event, that attached to specific game object;
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="eventCode"></param>
        /// <param name="listener"></param>
        public void RemoveListener(GameObject gameObject, int eventCode, Action<EventParams> listener)
        {
            if (gameObject == null)
            {
                CoreLog.LogError("Trying to remove listener, but target game object in null");
                return;
            }

            var instanceId = gameObject.GetInstanceID();

            //Check if we got target game object.
            if (m_GameObjectEventCollection.ContainsKey(instanceId))
            {
                //Get event collection for this object;
                var c = m_GameObjectEventCollection[instanceId];

                //Remove event with event code if no listeners;
                c.RemoveListener(eventCode, listener);

                //Check if game object event collection not empty, if empty - remove game object reference;
                if (!c.Any())
                    m_GameObjectEventCollection.Remove(instanceId);
            }
        }

        public void PrintListenersFor(int eventCode)
        {
            m_DefaultEventCollection.PrintListenersFor(eventCode);
        }
    }
}

