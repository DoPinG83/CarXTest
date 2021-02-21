using System;

namespace Core.Common
{
    public interface ITask<T> where T : class, ITask<T>
    {
        Action<T> Complete { get; set; }
        Action<T> Error { get; set; }
    
        void Start();
        void Stop();
    }
}
