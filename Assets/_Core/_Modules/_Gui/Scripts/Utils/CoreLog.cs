using System;
using UnityEngine;

namespace Core.Utils
{
    public sealed class CoreLog : MonoBehaviour
    {
        class Message
        {
            public readonly LogType Type;
            public readonly string Condition;
            public readonly string StackTrace;

            public Message(LogType logType, string condition, string stackTrace)
            {
                Type = logType;
                Condition = condition;
                StackTrace = stackTrace;
            }

            public override string ToString()
            {
                return string.Format("[{0}] {1} : {2}", Type, Condition, StackTrace);
            }

            public string ToColoredString()
            {
                var c = "gray";
                switch (Type)
                {
                    case LogType.Warning:
                        c = "yellow";
                        break;

                    case LogType.Error:
                    case LogType.Exception:
                        c = "red";
                        break;
                }

                return string.Format("<color={0}>{1}</color>", c, ToString());
            }
        }

        const int c_BufferSize = 100;

        static ExtendedStack<Message> s_CurrentMessages;

        public static bool GUIEnabled { get; set; }
        public static bool HandlingEnabled { get; private set; }
        public static LogType LogFilter { get; private set; }

        public static void SetLogFilter(LogType type)
        {
            LogFilter = type;
        }

        public static void SetHandlingEnabled(bool value)
        {
            HandlingEnabled = value;
            Application.logMessageReceived -= HandleLog;

            if (HandlingEnabled)
                Application.logMessageReceived += HandleLog;
        }

        public static void Clear()
        {
            s_CurrentMessages.Clear();
        }

        void RemoveAll(Func<Message, bool> predicate = null)
        {
            s_CurrentMessages.RemoweAll(predicate);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            DontDestroyOnLoad(new GameObject("CoreLog [Created in runtime]").AddComponent<CoreLog>());
            s_CurrentMessages = new ExtendedStack<Message>(c_BufferSize);

            SetHandlingEnabled(true);
            SetLogFilter(LogType.Error);

            GUIEnabled = false;

#if UNITY_LOG && !RELEASE_BUILD
            Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif
        }

        void OnDestroy()
        {
            SetHandlingEnabled(false);
        }

        static void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (type <= LogFilter || type == LogType.Exception)
            {
                var newMessage = new Message(type, condition, stackTrace);
                s_CurrentMessages.Push(new Message(type, condition, stackTrace));
#if !UNITY_EDITOR
                //_logWriter.OnLogging(condition, stackTrace, type);
#endif
            }
        }

        static void CheckLog(Action a)
        {
#if CORE_LOG && !RELEASE_BUILD
#if !UNITY_LOG
            Debug.unityLogger.logEnabled = true;
#endif
            a?.Invoke();
#if !UNITY_LOG
            Debug.unityLogger.logEnabled = false;
#endif
#endif
        }

        #region DEBUG_WRAPPER
        public static void Log(object message)
        {
            Action a = () =>
            {
                Debug.Log(message.ToString());
            };
            CheckLog(a);
        }

        public static void Log(string message)
        {
            Action a = () =>
            {
                Debug.Log(message);
            };
            CheckLog(a);
        }

        public static void Log(string message, UnityEngine.Object context)
        {
            Action a = () =>
            {
                Debug.Log(message, context);
            };
            CheckLog(a);
        }

        public static void LogWarning(string message)
        {
            Action a = () =>
            {
                Debug.LogWarning(message);
            };
            CheckLog(a);
        }

        public static void LogWarning(string message, UnityEngine.Object context)
        {
            Action a = () =>
            {
                Debug.LogWarning(message, context);
            };
            CheckLog(a);
        }

        public static void LogError(string message)
        {
            Action a = () =>
            {
                Debug.LogError(message);
            };
            CheckLog(a);
        }

        public static void LogError(string message, UnityEngine.Object context)
        {
            Action a = () =>
            {
                Debug.LogError(message, context);
            };
            CheckLog(a);
        }

        public static void LogFormat(string format, params object[] args)
        {
            Action a = () =>
            {
                Debug.LogFormat(format, args);
            };
            CheckLog(a);
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            Action a = () =>
            {
                Debug.LogWarningFormat(format, args);

            };
            CheckLog(a);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            Action a = () =>
            {
                Debug.LogErrorFormat(format, args);
            };
            CheckLog(a);
        }

        public static void LogException(System.Exception e)
        {
            Action a = () =>
            {
                Debug.LogException(e);
            };
            CheckLog(a);
        }
#endregion
    }
}