using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if PROFILE
using UnityEngine.Profiling;
#endif
using UnityEngine.UI;

namespace Core.Gui.Canvases
{
    public class StaticCanvas : CanvasBase
    {
#if UNITY_EDITOR
        [SerializeField]
        bool emulateIPhoneXSafeArea;
        public bool EmulateIPhoneXSafeArea => emulateIPhoneXSafeArea;

        [SerializeField]
        Rect safeAreaRect = new Rect(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f);
        public Rect IPhoneXSafeAreaRect => new Rect(Screen.width * safeAreaRect.x, Screen.height * safeAreaRect.y, Screen.width * safeAreaRect.width, Screen.height * safeAreaRect.height);
#endif

        [SerializeField] private CanvasGroup fadeCanvasGroup;
        public CanvasGroup FadeCanvasGroup => fadeCanvasGroup;

        public static StaticCanvas Instance { get; private set; }

        private bool _inputBlocked;
        public void SetInputBlock(bool blocked) => _inputBlocked = blocked;

        int[] trackedStates = new int[]
        {
            Animator.StringToHash("Open"),
            Animator.StringToHash("Close"),
        };

        protected override void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            base.Awake();
        }

        private void OnDestroy()
        {
        }

        public Vector2 ReferenceResolution
        {
            get
            {
                if (_canvasScaler == null)
                    _canvasScaler = GetComponent<CanvasScaler>();
                return _canvasScaler.referenceResolution;
            }
        }

        IEnumerable<Animator> currentAnimators = Enumerable.Empty<Animator>();
        int childCount = 0;
        CanvasGroup canvasGroup;

        private void Update()
        {
            if (childCount != transform.childCount)
            {
#if PROFILE
                Profiler.BeginSample("[StaticCanvas] Update animators");
#endif
                childCount = transform.childCount;
                currentAnimators = GetComponentsInChildren<Animator>()
                    .Where(a => a != null && trackedStates.All(s => a.HasState(0, s)));

                return; // Skip this frame;
#if PROFILE
                Profiler.EndSample();
#endif
            }

#if PROFILE
            Profiler.BeginSample("[StaticCanvas] Update");
#endif
            var isPlaying = _inputBlocked;
            foreach(var a in currentAnimators)
            {
                if (a == null)
                    continue;
                
                var s = a.GetCurrentAnimatorStateInfo(0);
                if (s.loop || s.speed < Mathf.Epsilon || !trackedStates.Contains(s.shortNameHash))
                    continue;

                if(s.normalizedTime < 1f - Mathf.Epsilon && s.normalizedTime > Mathf.Epsilon)
                {
                    isPlaying = true;
                    break;
                }
            }

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.interactable = !isPlaying;

#if PROFILE
            Profiler.EndSample();
#endif
        }
    }
}
