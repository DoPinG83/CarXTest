using Core.Common;

namespace Core.Game.Initialization
{
    /// <summary>
    /// Таска имеющая вес для отображения прогресса
    /// </summary>
    public interface ILoadingTask : ITask<ILoadingTask>
    {
        /// <summary>
        /// Вес таски
        /// </summary>
        int Weight { get; }

        /// <summary>
        /// Сообщение об ошибке, если таска сфейлилась
        /// </summary>
        string ErrorMsg { get; }
        string ErrorCode { get; }

        /// <summary>
        /// Текущий прогресс таски, -1 для не определен
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// null by default
        /// </summary>
        string LoadingText { get; }

        bool SendLoadingEvent { get; }
    }
}
