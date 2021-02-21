namespace Core.Common {
    using System;
    using System.Collections.Generic;
    using UniRx;

    /// <summary>
    /// Container for resolving global contract references.
    /// </summary>
    public sealed class Container : IDisposable {
        private readonly Dictionary<Type, Contract> _contracts = new Dictionary<Type, Contract>();
        private readonly Subject<Contract> _newContract = new Subject<Contract>();

        /// <summary>
        /// Registrate contract in single instance.
        /// If contract exist throw <see cref="InvalidOperationException"/>
        /// </summary>
        /// <param name="contract">Instance contract for registration</param>
        /// <returns>Return <see cref="IDisposable"/> for unscribing</returns>
        public IDisposable Register(Contract contract) {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            return Register(contract, contract.GetType());
        }

        private IDisposable Register(Contract contract, Type type) {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var container = new CompositeDisposable();
            RegisterLocal(contract, type).AddTo(container);
            return container;
        }

        private IDisposable RegisterLocal(Contract contract, Type type) {
            if (!_contracts.ContainsKey(type) && !_contracts.ContainsValue(contract)) {
                _contracts.Add(type, contract);
                _newContract.OnNext(contract);
                return Disposable.Create(() => Unregister(type));
            }

            throw new InvalidOperationException($"Current type {type} of contract is exists in container");
        }

        private void Unregister(Type type) {
            if (_contracts.ContainsKey(type)) {
                _contracts.Remove(type);
            }
        }

        /// <summary>
        /// Resolve contract <typeparamref name="T"/> synchronously.
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <returns>Instance of <typeparamref name="T"/> or null</returns>
        public T Resolve<T>() where T : Contract {
            Contract contract;
            if (_contracts.TryGetValue(typeof(T), out contract)) {
                return (T) contract;
            }

            return null;
        }

        /// <summary>
        /// Resolve contact <typeparamref name="T"/> asynchronously. 
        /// Waiting for contract if contract doesn't exist.
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        public IObservable<T> ResolveAsync<T>() where T : Contract {
            return Observable.Create<T>(o => {
                Contract contract;
                if (_contracts.TryGetValue(typeof(T), out contract)) {
                    o.OnNext((T) contract);
                    o.OnCompleted();
                    return Disposable.Empty;
                }

                return _newContract
                    .Where(c => c is T)
                    .Subscribe(c => {
                        o.OnNext((T) c);
                        o.OnCompleted();
                    });
            });
        }

        /// <summary>
        /// Stream contracts with <typeparamref name="T"/>.
        /// <para/>Doesn't work with async\await cause never calling OnComplete().
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        public IObservable<T> ResolveStream<T>() where T : Contract {
            return Observable.Create<T>(o => {
                Contract contract;
                if (_contracts.TryGetValue(typeof(T), out contract)) {
                    o.OnNext((T) contract);
                }

                return _newContract
                    .Where(c => c is T)
                    .Subscribe(c => o.OnNext((T) c));
            });
        }

        /// <summary>
        /// Clear contracts list without locking container
        /// </summary>
        public void Dispose() {
            _contracts.Clear();
        }
    }
}