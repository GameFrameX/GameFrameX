// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;

namespace Animancer.FSM
{
    /// <summary>A static access point for the details of a key change in a <see cref="StateMachine{TKey, TState}"/>.</summary>
    /// <remarks>
    /// This system is thread-safe.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/changing-states">Changing States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/KeyChange_1
    /// 
    public struct KeyChange<TKey> : IDisposable
    {
        /************************************************************************************************************************/

        [ThreadStatic]
        private static KeyChange<TKey> _Current;

        private IKeyedStateMachine<TKey> _StateMachine;
        private TKey _PreviousKey;
        private TKey _NextKey;

        /************************************************************************************************************************/

        /// <summary>Is a <see cref="KeyChange{TKey}"/> of this type currently occurring?</summary>
        public static bool IsActive => _Current._StateMachine != null;

        /// <summary>The <see cref="KeyChange{TKey}"/> in which the current change is occurring.</summary>
        /// <remarks>This will be null if no change is currently occurring.</remarks>
        public static IKeyedStateMachine<TKey> StateMachine => _Current._StateMachine;

        /************************************************************************************************************************/

        /// <summary>The key being changed from.</summary>
        /// <exception cref="InvalidOperationException">[Assert-Only]
        /// <see cref="IsActive"/> is false so this property is likely being accessed on the wrong generic type.
        /// </exception>
        public static TKey PreviousKey
        {
            get
            {
#if UNITY_ASSERTIONS
                if (!IsActive)
                    throw new InvalidOperationException(StateExtensions.GetChangeError(typeof(TKey), typeof(StateMachine<,>), "Key"));
#endif
                return _Current._PreviousKey;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The key being changed into.</summary>
        /// <exception cref="InvalidOperationException">[Assert-Only]
        /// <see cref="IsActive"/> is false so this property is likely being accessed on the wrong generic type.
        /// </exception>
        public static TKey NextKey
        {
            get
            {
#if UNITY_ASSERTIONS
                if (!IsActive)
                    throw new InvalidOperationException(StateExtensions.GetChangeError(typeof(TKey), typeof(StateMachine<,>), "Key"));
#endif
                return _Current._NextKey;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Assigns the parameters as the details of the currently active change and creates a new
        /// <see cref="KeyChange{TKey}"/> containing the details of the previously active change so that disposing
        /// it will re-assign those previous details to be current again in case of recursive state changes.
        /// </summary>
        /// <example><code>
        /// using (new KeyChange&lt;TState&gt;(previousKey, nextKey))
        /// {
        ///     // Do the actual key change.
        /// }
        /// </code></example>
        internal KeyChange(IKeyedStateMachine<TKey> stateMachine, TKey previousKey, TKey nextKey)
        {
            this = _Current;

            _Current._StateMachine = stateMachine;
            _Current._PreviousKey = previousKey;
            _Current._NextKey = nextKey;
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IDisposable"/>]
        /// Re-assigns the values of this change (which were the previous values from when it was created) to be the
        /// currently active change. See the constructor for recommended usage.
        /// </summary>
        /// <remarks>
        /// Usually this will be returning to default values (nulls), but if one state change causes another then the
        /// second one ending will return to the first which will then return to the defaults.
        /// </remarks>
        public void Dispose()
        {
            _Current = this;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string describing the contents of this <see cref="KeyChange{TKey}"/>.</summary>
        public override string ToString() => IsActive ?
            $"{nameof(KeyChange<TKey>)}<{typeof(TKey).FullName}" +
            $">({nameof(PreviousKey)}={PreviousKey}" +
            $", {nameof(NextKey)}={NextKey})" :
            $"{nameof(KeyChange<TKey>)}<{typeof(TKey).FullName}(Not Currently Active)";

        /// <summary>Returns a string describing the contents of the current <see cref="KeyChange{TKey}"/>.</summary>
        public static string CurrentToString() => _Current.ToString();

        /************************************************************************************************************************/
    }
}
