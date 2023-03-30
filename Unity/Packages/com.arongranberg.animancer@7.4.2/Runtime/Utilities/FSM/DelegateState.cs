// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;

namespace Animancer.FSM
{
    /// <summary>An <see cref="IState"/> that uses delegates to define its behaviour.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/state-types">State Types</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/DelegateState
    /// 
    public class DelegateState : IState
    {
        /************************************************************************************************************************/

        /// <summary>Determines whether this state can be entered. Null is treated as returning true.</summary>
        public Func<bool> canEnter;

        /// <summary>[<see cref="IState"/>] Calls <see cref="canEnter"/> to determine whether this state can be entered.</summary>
        public virtual bool CanEnterState => canEnter == null || canEnter();

        /************************************************************************************************************************/

        /// <summary>Determines whether this state can be exited. Null is treated as returning true.</summary>
        public Func<bool> canExit;

        /// <summary>[<see cref="IState"/>] Calls <see cref="canExit"/> to determine whether this state can be exited.</summary>
        public virtual bool CanExitState => canExit == null || canExit();

        /************************************************************************************************************************/

        /// <summary>Called when this state is entered.</summary>
        public Action onEnter;

        /// <summary>[<see cref="IState"/>] Calls <see cref="onEnter"/> when this state is entered.</summary>
        public virtual void OnEnterState() => onEnter?.Invoke();

        /************************************************************************************************************************/

        /// <summary>Called when this state is exited.</summary>
        public Action onExit;

        /// <summary>[<see cref="IState"/>] Calls <see cref="onExit"/> when this state is exited.</summary>
        public virtual void OnExitState() => onExit?.Invoke();

        /************************************************************************************************************************/
    }
}
