namespace Core.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Base non-generic type for contracts
    /// </summary>
    public abstract class Contract : IDisposable {
        /// <summary>
        /// Reference for object with which the contract is associated
        /// </summary>
        public object Target { get; protected set; }

        /// <summary>
        /// Unique indentifier for getting different instances from single target
        /// </summary>
        public string Identifier { get; protected set; }

        /// <summary>
        /// Default place for getting sub-contracts or create complex <see cref="IObservable{T}"/>
        /// </summary>
        protected virtual void Initialize() {
        }

        public virtual void Dispose() {
        }

        [AttributeUsage(AttributeTargets.All)]
        protected class DescriptionAttribute : Attribute {
            protected DescriptionAttribute() : this("") {
            }

            protected DescriptionAttribute(string description) {
                Description = description;
            }

            public string Description { get; }
        }

        protected sealed class InputAttribute : DescriptionAttribute {
            public InputAttribute() {
            }

            public InputAttribute(string description) : base(description) {
            }
        }

        protected sealed class OutputAttribute : DescriptionAttribute {
            public OutputAttribute() {
            }

            public OutputAttribute(string description) : base(description) {
            }
        }

        protected sealed class InternalAttribute : DescriptionAttribute {
            public InternalAttribute() {
            }

            public InternalAttribute(string description) : base(description) {
            }
        }
    }

    /// <summary>
    /// Mediator for <see cref="UniRx.ReactiveCommand"/>, <see cref="UniRx.ReactiveProperty{T}"/>, <see cref="UniRx.ReactiveOperation{T,TR}"/>
    /// </summary>
    /// <typeparam name="T0">Type of inherited contract</typeparam>
    public abstract class Contract<T0> : Contract where T0 : Contract<T0>, new() {
        private static readonly List<T0> Contracts = new List<T0>();

        protected Contract() {
        }

        /// <summary>
        /// Special constructor for creating non-gameObject contracts. Also for static types.
        /// </summary>
        /// <param name="target"></param>
        public Contract(object target) {
            Target = target;
            Initialize();
            Contracts.Add((T0) this);
        }

        /// <summary>
        /// Return instance <see cref="Contract{T0}"/> or create it, if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T0">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T0 Get(Component component, string identifier = "") {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return Get(component.gameObject, identifier);
        }

        /// <summary>
        /// Return instance <see cref="Contract{T0}"/> or create it, if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T0">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T0 Get(GameObject gameObject, string identifier = "") {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
            return Get((object) gameObject, identifier);
        }

        /// <summary>
        /// Return instance <see cref="Contract{T0}"/> or create it, if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T0">Type of contract</typeparam>
        /// <param name="obj">Usual type or null</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T0 Get(object obj, string identifier = "") {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var contract = TryGet(obj, identifier);
            if (contract == null) {
                contract = new T0 {
                    Target = obj,
                    Identifier = identifier
                };
                contract.Initialize();
                Contracts.Add(contract);
            }

            return contract;
        }

        /// <summary>
        /// Return instance <see cref="Contract{T0}"/> or null if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T0">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T0 TryGet(Component component, string identifier = "") {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return TryGet(component.gameObject, identifier);
        }

        /// <summary>
        /// Return instance <see cref="Contract{T0}"/> or null if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T0">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T0 TryGet(GameObject gameObject, string identifier = "") {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
            return TryGet((object) gameObject, identifier);
        }

        /// <summary>
        /// Return instance <see cref="Contract{T0}"/> or null if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T0">Type of contract</typeparam>
        /// <param name="obj">Usual type or null</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T0 TryGet(object obj, string identifier = "") {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return Contracts.FirstOrDefault(c => c.Target == obj && c.Identifier == identifier);
        }

        protected T1 GetSub<T1>(string identifier = "") where T1 : Contract<T1>, new() {
            var local = Contract<T1>.Get(Target, identifier);
            return local;
        }

        public override void Dispose() {
            Contracts.Remove((T0) this);
            base.Dispose();
        }
    }
}