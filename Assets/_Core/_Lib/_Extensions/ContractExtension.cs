namespace Core.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using Common;
    using UniRx;
    using UnityEngine;

    public static class ContractExtension {
        /// <summary>
        /// Return instance <see cref="Contract{T}"/> or null if it doesn't exist for current object
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T TryGet<T>(this Component component, string identifier = "") where T : Contract<T>, new() {
            return Contract<T>.TryGet(component, identifier);
        }

        /// <summary>
        /// Return instance <see cref="Contract{T}"/> or null if it doesn't exist for current object
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T TryGet<T>(this GameObject gameObject, string identifier = "") where T : Contract<T>, new() {
            return Contract<T>.TryGet(gameObject, identifier);
        }

        /// <summary>
        /// Return instance <see cref="Contract{T}"/> or create it, if it doesn't exist on current gameObject
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T Get<T>(this Component component, string identifier = "") where T : Contract<T>, new() {
            return Contract<T>.Get(component, identifier);
        }


        /// <summary>
        /// Return instance <see cref="Contract{T}"/> or create it, if it doesn't exist on current gameObject
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifer for contract</param>
        public static T Get<T>(this GameObject gameObject, string identifier = "") where T : Contract<T>, new() {
            return Contract<T>.Get(gameObject, identifier);
        }
    }
}