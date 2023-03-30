// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;

namespace Animancer.FSM
{
    /// <summary>A static access point for the details of a state change in a <see cref="StateMachine{TState}"/>.</summary>
    /// <remarks>
    /// This system is thread-safe.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/changing-states">Changing States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateChange_1
    /// 
    public struct StateChange<TState> : IDisposable where TState : class, IState
    {
        /************************************************************************************************************************/

        [ThreadStatic]
        private static StateChange<TState> _Current;

        private StateMachine<TState> _StateMachine;
        private TState _PreviousState;
        private TState _NextState;

        /************************************************************************************************************************/

        /// <summary>Is a <see cref="StateChange{TState}"/> of this type currently occurring?</summary>
        public static bool IsActive => _Current._StateMachine != null;

        /// <summary>The <see cref="StateMachine{TState}"/> in which the current change is occurring.</summary>
        /// <remarks>This will be null if no change is currently occurring.</remarks>
        public static StateMachine<TState> StateMachine => _Current._StateMachine;

        /************************************************************************************************************************/

        /// <summary>The state currently being changed from.</summary>
        /// <exception cref="InvalidOperationException">[Assert-Only]
        /// <see cref="IsActive"/> is false so this property is likely being accessed on the wrong generic type.
        /// </exception>
        public static TState PreviousState
        {
            get
            {
#if UNITY_ASSERTIONS
                if (!IsActive)
                    throw new InvalidOperationException(StateExtensions.GetChangeError(typeof(TState), typeof(StateMachine<>)));
#endif
                return _Current._PreviousState;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The state being changed into.</summary>
        /// <exception cref="InvalidOperationException">[Assert-Only]
        /// <see cref="IsActive"/> is false so this property is likely being accessed on the wrong generic type.
        /// </exception>
        public static TState NextState
        {
            get
            {
#if UNITY_ASSERTIONS
                if (!IsActive)
                    throw new InvalidOperationException(StateExtensions.GetChangeError(typeof(TState), typeof(StateMachine<>)));
#endif
                return _Current._NextState;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Assigns the parameters as the details of the currently active change and creates a new
        /// <see cref="StateChange{TState}"/> containing the details of the previously active change so that disposing
        /// it will re-assign those previous details to be current again in case of recursive state changes.
        /// </summary>
        /// <example><code>
        /// using (new StateChange&lt;TState&gt;(stateMachine, previousState, nextState))
        /// {
        ///     // Do the actual state change.
        /// }
        /// </code></example>
        internal StateChange(StateMachine<TState> stateMachine, TState previousState, TState nextState)
        {
            this = _Current;

            _Current._StateMachine = stateMachine;
            _Current._PreviousState = previousState;
            _Current._NextState = nextState;
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

        /// <summary>Returns a string describing the contents of this <see cref="StateChange{TState}"/>.</summary>
        public override string ToString() => IsActive ?
            $"{nameof(StateChange<TState>)}<{typeof(TState).FullName}" +
            $">({nameof(PreviousState)}='{_PreviousState}'" +
            $", {nameof(NextState)}='{_NextState}')" :
            $"{nameof(StateChange<TState>)}<{typeof(TState).FullName}(Not Currently Active)";

        /// <summary>Returns a string describing the contents of the current <see cref="StateChange{TState}"/>.</summary>
        public static string CurrentToString() => _Current.ToString();

        /************************************************************************************************************************/
    }
}
