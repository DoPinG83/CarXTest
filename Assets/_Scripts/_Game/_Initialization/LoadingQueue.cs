using System;
using Core.Common;

namespace Core.Game.Initialization
{
    /// <summary>
    /// Очередь тасок для отображения прогресса по весам выполняемых задач
    /// </summary>
    public class LoadingQueue : TaskQueue<ILoadingTask>
    {
        public Action<ILoadingTask> TaskDone;
        private int _weight = 0;
        private int _i;
        private float _progressCurrentTasks;
        private int _countWithProgress;

        public LoadingQueue(int maxThreads, TaskQueueMode mode)
            : base(maxThreads, mode)
        {
        }

        protected override void OnTaskComplete(ILoadingTask task)
        {
            if (TaskDone != null)
                TaskDone(task);
            base.OnTaskComplete(task);
        }

        /// <summary>
        /// Суммарный вес выполняемых тасок
        /// </summary>
        public int Weight
        {
            get
            {
                return WeightRunning + WeightQueued;
            }
        }

        public int WeightRunning
        {
            get
            {
                _weight = 0;
                for (_i = 0; _i < _runningTasks.Count; ++_i)
                    _weight += _runningTasks[_i].Weight;
                return _weight;
            }
        }

        public int WeightQueued
        {
            get
            {
                _weight = 0;
                for (_i = 0; _i < _queuedTasks.Count; ++_i)
                    _weight += _queuedTasks[_i].Weight;
                return _weight;
            }
        }

        public float ProgressCurrentTasks
        {
            get
            {
                if (_runningTasks.Count > 0)
                {
                    _progressCurrentTasks = 0;
                    _countWithProgress = 0;
                    for (_i = 0; _i < _runningTasks.Count; ++_i)
                    {
                        if (_runningTasks[_i].Progress >= 0)
                        {
                            _progressCurrentTasks += _runningTasks[_i].Progress;
                            _countWithProgress++;
                        }
                    }
                    if (_countWithProgress > 0)
                    {
                        return _progressCurrentTasks / _countWithProgress;
                    }
                }
                return -1;
            }
        }

        public string LoadingText
        {
            get
            {
                for (_i = 0; _i < _runningTasks.Count; ++_i)
                {
                    string text = _runningTasks[_i].LoadingText;
                    if (text != null)
                        return text;
                }
                return null;
            }
        }
    }
}
