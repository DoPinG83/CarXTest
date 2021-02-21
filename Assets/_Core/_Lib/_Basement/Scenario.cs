namespace Core.Common {
    public abstract class Scenario<T> {
        public abstract void Run(T args);
    }
}