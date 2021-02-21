using System.Collections;
using Core.Common.Modules;
using Core.Common;
using Core.Gui;
using Core.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.Game.Initialization.Gui
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] string _prodSceneName;
        [SerializeField] RectTransform _background;
        [SerializeField] Image _progressValue;
        [SerializeField] Text _loadingText;

        private float _fakeProgressAnimationTotalTime = 40f;    // время, за которое фейковый прогресс заполнился бы полностью (если б мог)

        private LoadingQueue _queue;

        private int _maxTaskWeight;
        private float _startAt;
        private float _queueProgress;
        private float _queueProgressNormalized;
        private float _progressCurrent = -1;
        private float _progressNext;

        private string _loadingTextDefaultLocale;

        private bool _isLoadingComplete;

        public float LoadingProgress
        {
            get { return _isLoadingComplete ? 1f : _queue == null ? 0f : Mathf.Max(_queue.ProgressCurrentTasks, 0f); }
        }

        public string LoadingText
        {
            get { return _queue == null ? "" : _queue.LoadingText; }
        }

        // Start is called before the first frame update
        void Start()
        {
            var backTexture = _background.GetComponent<Image>().sprite.texture;
            var texWidth = (float)backTexture.width;
            var texHeight = (float)backTexture.height;
            var coef = Screen.height / texHeight; 
            _background.sizeDelta = new Vector2(texWidth * coef, _background.sizeDelta.y);

            _loadingTextDefaultLocale ="Loading...";

            // создаем очередь тасок на инициализацию
            _queue = new LoadingQueue(1, TaskQueueMode.StopOnError);
            _queue.TaskDone += QueueOnTaskDone;

            _queue.Add(new SceneLoadingTask(_prodSceneName));
            _queue.Add(new RegisterManagersTask());

           _queue.Complete += OnQueueComplete;
            _queue.Error += OnQueueError;

            _startAt = 0.0f;
            _maxTaskWeight = _queue.Weight;

            _progressValue.fillAmount = _startAt;

            _queue.Start();
        }

        void Update()
        {
            if(_progressValue.fillAmount == 1f)
            {
                if (!_isLoadingComplete)
                {
                    QueueComplete();
                }
                return;
            }

            float progressCurrentTasks = _queue.ProgressCurrentTasks;
            float progressNew = GetProgress(_queue.Weight);

            var newProgressValue = _progressValue.fillAmount;
            if (progressNew > _progressCurrent)
            {
                _progressCurrent = progressNew;
                _progressNext = GetProgress(_queue.WeightQueued); // вес текущих минус выполняемые

                if (progressCurrentTasks < 0)
                {
                    float fakeAnimationTime = (_queue.Weight - _queue.WeightQueued) / (float)_maxTaskWeight * _fakeProgressAnimationTotalTime;
                   FakeProgressAnimate(_progressCurrent, _progressNext, fakeAnimationTime);
                }

                newProgressValue = _progressCurrent;
            }

            if (progressCurrentTasks >= 0)
            {
                newProgressValue = _progressCurrent + (_progressNext - _progressCurrent) * progressCurrentTasks;
            }

            _progressValue.fillAmount = newProgressValue;//Mathf.Lerp(_progressValue.fillAmount, newProgressValue, Time.deltaTime);

            string loadingText = _queue.LoadingText != null ? _queue.LoadingText : _loadingTextDefaultLocale;
            _loadingText.text = loadingText;
        }

        private float GetProgress(float weight)
        {
            _queueProgress = ((float)_maxTaskWeight - weight) / _maxTaskWeight;
            _queueProgressNormalized = (1.0f - _startAt) * _queueProgress;
            return _startAt + _queueProgressNormalized;
        }

        private IEnumerator _fakeProgressAnimation;
        private void FakeProgressAnimate(float from, float to, float time)
        {
            StopFakeProgressAnimate();

            _fakeProgressAnimation = FakeProgressAnimation(from, to, time);
            StartCoroutine(_fakeProgressAnimation);
        }
        private void StopFakeProgressAnimate()
        {
            if (_fakeProgressAnimation != null)
            {
                StopCoroutine(_fakeProgressAnimation);
                _fakeProgressAnimation = null;
            }
        }
        private IEnumerator FakeProgressAnimation(float from, float to, float time)
        {
            float timePass = 0;
            while (timePass < time)
            {
                _progressValue.fillAmount = Mathf.Lerp(from, to, timePass / time);
                yield return null;
                timePass += Time.deltaTime;
            }
            _progressValue.fillAmount = to;
        }

        private void QueueOnTaskDone(ILoadingTask loadingTask)
        {
            CoreLog.Log($"QueueOnTaskDone {loadingTask.GetType().Name}");
        }

        private void OnQueueComplete(ILoadingTask task)
        {
            CoreLog.Log("OnQueueComplete");
        }

        private void OnQueueError(ILoadingTask failedTask)
        {
            _isLoadingComplete = false;
            QueueComplete();
        }

        public void StopQueue()
        {
            _isLoadingComplete = false;

            if (_queue == null)
                return;

            _queue.StopImmediately();
            _queue.Clear();

            QueueComplete();
        }

        void QueueComplete()
        {
            _isLoadingComplete = true;
            if (_queue != null)
            {
                _queue.Complete -= OnQueueComplete;
                _queue.Error -= OnQueueError;
                _queue.TaskDone -= QueueOnTaskDone;
                _queue = null;
            }

            var gui = Modules.All.Resolve<CGUI>();
            gui.DoFadeIn.Execute((1f,
                () => {
                    Broadcaster.Instance.Invoke(BroadcasterEventCodes.LoadingQueueFinished, new EventParams());
                    SceneManager.UnloadScene(0);
                }
            ));
        }
    }
}
