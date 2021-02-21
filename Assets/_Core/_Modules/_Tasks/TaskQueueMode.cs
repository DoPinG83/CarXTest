namespace Core.Common
{
    public enum TaskQueueMode
    {
        /// <summary>
        /// Выполняет все таски в очереди вне зависимости от результата выполнения каждой конкретной таски
        /// </summary>
        RunAll,
        /// <summary>
        /// Останавливает очередь как только какая-либо таска усешно завершится
        /// </summary>
        StopOnComplete,
        /// <summary>
        /// Останавливает очередь как только при выполнении какой-либо таски произойдет ошибка
        /// </summary>
        StopOnError
    }
}
