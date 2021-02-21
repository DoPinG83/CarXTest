using System;
using System.Collections.Generic;

namespace Core.Common
{
    /// <summary>
    /// очередь выполнения тасков. является локальной очередью(не используется как единая очередь дя всех тасков системы)
    /// </summary>
    public class TaskQueue<TTask> where TTask : class, ITask<TTask>
    {
        public Action<TTask> Complete;
        public Action<TTask> Error;

        protected readonly List<TTask> _queuedTasks = new List<TTask>();
        protected readonly List<TTask> _runningTasks = new List<TTask>();
        protected readonly List<int> _runningTasksTimeStart = new List<int>();  //in milliseconds

        protected bool _running;

        protected int _maxThreads;
        protected readonly TaskQueueMode _mode;

        public ICollection<TTask> GetRunningTasks()
        {
            return _runningTasks.ToArray();
        }

        public ICollection<TTask> GetAllTasks()
        {
            List<TTask> tasks = new List<TTask>(_runningTasks);
            tasks.AddRange(_queuedTasks);
            return tasks.ToArray();
        }

        public int TaskCount { get { return _runningTasks.Count + _queuedTasks.Count; } }

        public List<int> GetRunningTasksTimeStart()
        {
            return _runningTasksTimeStart;
        }

        public int RunningTasks
        {
            get { return _runningTasks.Count; }
        }

        public int QueuedTasks
        {
            get { return _queuedTasks.Count; }
        }

        public bool IsRunning
        {
            get { return _running; }
        }

        public TaskQueue()
            : this(1, TaskQueueMode.RunAll)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxThreads">Максимальное число одновременно выполняющихся асинхронных заданий</param>
        /// <param name="mode">Режим выполнения тасок очереди</param>
        public TaskQueue(int maxThreads, TaskQueueMode mode)
        {
            _maxThreads = (maxThreads > 0) ? maxThreads : 1;
            _mode = mode;
            _running = false;
        }

        /// <summary>
        /// Запустить выполнение очереди заданий
        /// </summary>
        public void Start()
        {
            if (_running)
                return;
            _running = true;
            CheckQueue();
        }

        /// <summary>
        /// Остановить выполнение очереди заданий
        /// </summary>
        public void Stop()
        {
            if (!_running)
                return;
            _running = false;
        }

        /// <summary>
        /// Немедленно остановить очередь и все выполняемые задания
        /// </summary>
        public void StopImmediately()
        {
            foreach (TTask runningTask in _runningTasks)
            {
                Unsubscribe(runningTask);
                runningTask.Stop();
            }
            _runningTasks.Clear();
            _runningTasksTimeStart.Clear();
            Stop();
        }

        /// <summary>
        /// Удалить все отложенные задания
        /// </summary>
        public void Clear()
        {
            _queuedTasks.Clear();

            foreach (var task in _runningTasks)
                task.Stop();

            _runningTasks.Clear();
        }

        /// <summary>
        /// Добавить задание в очередь
        /// </summary>
        /// <param name="task"></param>
        public virtual TTask Add(TTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _queuedTasks.Add(task);

            // Запускаем задачу, если возможно
            CheckQueue();
            return task;
        }

        /// <summary>
        /// Добавить задание в очередь вперед остальных команд
        /// </summary>
        /// <param name="task"></param>
        public virtual TTask AddFront(TTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _queuedTasks.Insert(0, task);

            // Запускаем задачу, если возможно
            CheckQueue();
            return task;
        }

        /// <summary>
        /// Убрать задание из очереди (если задание выполняется, оно будет остановлено принудительно)
        /// </summary>
        /// <param name="task"></param>
        public void Remove(TTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            // Ищем задание в списке ожидающих
            if (_queuedTasks.Remove(task))
                return;

            // Ищем в выполняемых
            int index = _runningTasks.IndexOf(task);
            if (index == -1)
                return;

            // Останавливаем задачу
            Unsubscribe(task);
            _runningTasks.RemoveAt(index);
            _runningTasksTimeStart.RemoveAt(index);
            task.Stop();

            // Запускаем задачу, если возможно
            CheckQueue();
        }

        private void CheckQueue()
        {
            // пока есть ожидающие задания и выполняемых заданий меньше
            // чем число потоков - набиваем буфер и стартуем задания
            while (_running && (_queuedTasks.Count > 0) && (_runningTasks.Count < _maxThreads))
                StartNextTask();

            // Если нет выполняемых заданий, значит очередь завершена
            if (!_running || _runningTasks.Count != 0) 
                return;

            _running = false;
            if (Complete != null)
                Complete(null);
        }

        private void StartNextTask()
        {
            if (!_running || _queuedTasks.Count == 0)
                return;

            TTask task = _queuedTasks[0];
            _queuedTasks.RemoveAt(0);

            task.Complete += OnTaskComplete;
            task.Error += OnTaskError;

            _runningTasks.Add(task);
            _runningTasksTimeStart.Add(Environment.TickCount & Int32.MaxValue);
            task.Start();
        }

        private void Unsubscribe(TTask task)
        {
            task.Complete -= OnTaskComplete;
            task.Error -= OnTaskError;
        }

        protected virtual void OnTaskComplete(TTask task)
        {
            Unsubscribe(task);
            int index = _runningTasks.IndexOf(task);
            _runningTasks.RemoveAt(index);
            _runningTasksTimeStart.RemoveAt(index);

            if (_mode == TaskQueueMode.StopOnComplete)
            {
                StopImmediately();
                if (Complete != null)
                    Complete(task);
                return;
            }

            CheckQueue();
        }

        private void OnTaskError(TTask task)
        {
            Unsubscribe(task);
            int index = _runningTasks.IndexOf(task);
            _runningTasks.RemoveAt(index);
            _runningTasksTimeStart.RemoveAt(index);

            if (_mode == TaskQueueMode.StopOnError)
            {
                StopImmediately();
                if (Error != null)
                    Error(task);
                return;
            }

            CheckQueue();
        }
    }
}
