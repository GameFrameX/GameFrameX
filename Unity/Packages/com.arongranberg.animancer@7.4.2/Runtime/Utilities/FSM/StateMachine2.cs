// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer.FSM
{
    /// <summary>Interface for accessing <see cref="StateMachine{TKey, TState}"/> without the <c>TState</c>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/keys">Keyed State Machines</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/IKeyedStateMachine_1
    /// 
    public interface IKeyedStateMachine<TKey>
    {
        /************************************************************************************************************************/

        /// <summary>The key which identifies the <see cref="StateMachine{TState}.CurrentState"/>.</summary>
        TKey CurrentKey { get; }

        /// <summary>The <see cref="KeyChange{TKey}.PreviousKey"/>.</summary>
        TKey PreviousKey { get; }

        /// <summary>The <see cref="KeyChange{TKey}.NextKey"/>.</summary>
        TKey NextKey { get; }

        /// <summary>Attempts to enter the state registered with the specified `key` and returns it if successful.</summary>
        /// <remarks>
        /// This method returns true immediately if the specified `key` is already the <see cref="CurrentKey"/>. To
        /// allow directly re-entering the same state, use <see cref="TryResetState(TKey)"/> instead.
        /// </remarks>
        object TrySetState(TKey key);

        /// <summary>Attempts to enter the state registered with the specified `key` and returns it if successful.</summary>
        /// <remarks>
        /// This method does not check if the `key` is already the <see cref="CurrentKey"/>. To do so, use
        /// <see cref="TrySetState(TKey)"/> instead.
        /// </remarks>
        object TryResetState(TKey key);

        /// <summary>
        /// Uses <see cref="StateMachine{TKey, TState}.ForceSetState(TKey, TState)"/> to change to the state registered
        /// with the `key`. If nothing is registered, it changes to <c>default(TState)</c>.
        /// </summary>
        object ForceSetState(TKey key);

        /************************************************************************************************************************/
    }

    /// <summary>A simple Finite State Machine system that registers each state with a particular key.</summary>
    /// <remarks>
    /// This class allows states to be registered with a particular key upfront and then accessed later using that key.
    /// See <see cref="StateMachine{TState}"/> for a system that does not bother keeping track of any states other than
    /// the active one.
    /// <para></para>
    /// See <see cref="InitializeAfterDeserialize"/> if using this class in a serialized field.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/keys">Keyed State Machines</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateMachine_2
    /// 
    [HelpURL(StateExtensions.APIDocumentationURL + nameof(StateMachine<TState>) + "_2")]
    [Serializable]
    public partial class StateMachine<TKey, TState> : StateMachine<TState>, IKeyedStateMachine<TKey>, IDictionary<TKey, TState>
        where TState : class, IState
    {
        /************************************************************************************************************************/

        /// <summary>The collection of states mapped to a particular key.</summary>
        public IDictionary<TKey, TState> Dictionary { get; set; }

        /************************************************************************************************************************/

        [SerializeField]
        private TKey _CurrentKey;

        /// <summary>The key which identifies the <see cref="StateMachine{TState}.CurrentState"/>.</summary>
        public TKey CurrentKey => _CurrentKey;

        /************************************************************************************************************************/

        /// <summary>The <see cref="KeyChange{TKey}.PreviousKey"/>.</summary>
        public TKey PreviousKey => KeyChange<TKey>.PreviousKey;

        /// <summary>The <see cref="KeyChange{TKey}.NextKey"/>.</summary>
        public TKey NextKey => KeyChange<TKey>.NextKey;

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="StateMachine{TKey, TState}"/> with a new <see cref="Dictionary"/>, leaving the
        /// <see cref="CurrentState"/> null.
        /// </summary>
        public StateMachine()
        {
            Dictionary = new Dictionary<TKey, TState>();
        }

        /// <summary>
        /// Creates a new <see cref="StateMachine{TKey, TState}"/> which uses the specified `dictionary`, leaving the
        /// <see cref="CurrentState"/> null.
        /// </summary>
        public StateMachine(IDictionary<TKey, TState> dictionary)
        {
            Dictionary = dictionary;
        }

        /// <summary>
        /// Constructs a new <see cref="StateMachine{TKey, TState}"/> with a new <see cref="Dictionary"/> and
        /// immediately uses the `defaultKey` to enter the `defaultState`.
        /// </summary>
        /// <remarks>This calls <see cref="IState.OnEnterState"/> but not <see cref="IState.CanEnterState"/>.</remarks>
        public StateMachine(TKey defaultKey, TState defaultState)
        {
            Dictionary = new Dictionary<TKey, TState>
            {
                { defaultKey, defaultState }
            };
            ForceSetState(defaultKey, defaultState);
        }

        /// <summary>
        /// Constructs a new <see cref="StateMachine{TKey, TState}"/> which uses the specified `dictionary` and
        /// immediately uses the `defaultKey` to enter the `defaultState`.
        /// </summary>
        /// <remarks>This calls <see cref="IState.OnEnterState"/> but not <see cref="IState.CanEnterState"/>.</remarks>
        public StateMachine(IDictionary<TKey, TState> dictionary, TKey defaultKey, TState defaultState)
        {
            Dictionary = dictionary;
            dictionary.Add(defaultKey, defaultState);
            ForceSetState(defaultKey, defaultState);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void InitializeAfterDeserialize()
        {
            if (CurrentState != null)
            {
                using (new KeyChange<TKey>(this, default, _CurrentKey))
                using (new StateChange<TState>(this, null, CurrentState))
                    CurrentState.OnEnterState();
            }
            else if (Dictionary.TryGetValue(_CurrentKey, out var state))
            {
                ForceSetState(_CurrentKey, state);
            }

            // Don't call the base method.
        }

        /************************************************************************************************************************/

        /// <summary>Attempts to enter the specified `state` and returns true if successful.</summary>
        /// <remarks>
        /// This method returns true immediately if the specified `state` is already the
        /// <see cref="StateMachine{TState}.CurrentState"/>. To allow directly re-entering the same state, use
        /// <see cref="TryResetState(TKey, TState)"/> instead.
        /// </remarks>
        public bool TrySetState(TKey key, TState state)
        {
            if (CurrentState == state)
                return true;
            else
                return TryResetState(key, state);
        }

        /// <summary>Attempts to enter the state registered with the specified `key` and returns it if successful.</summary>
        /// <remarks>
        /// This method returns true immediately if the specified `key` is already the <see cref="CurrentKey"/>. To
        /// allow directly re-entering the same state, use <see cref="TryResetState(TKey)"/> instead.
        /// </remarks>
        public TState TrySetState(TKey key)
        {
            if (EqualityComparer<TKey>.Default.Equals(_CurrentKey, key))
                return CurrentState;
            else
                return TryResetState(key);
        }

        /// <inheritdoc/>
        object IKeyedStateMachine<TKey>.TrySetState(TKey key) => TrySetState(key);

        /************************************************************************************************************************/

        /// <summary>Attempts to enter the specified `state` and returns true if successful.</summary>
        /// <remarks>
        /// This method does not check if the `state` is already the <see cref="StateMachine{TState}.CurrentState"/>.
        /// To do so, use <see cref="TrySetState(TKey, TState)"/> instead.
        /// </remarks>
        public bool TryResetState(TKey key, TState state)
        {
            using (new KeyChange<TKey>(this, _CurrentKey, key))
            {
                if (!CanSetState(state))
                    return false;

                _CurrentKey = key;
                ForceSetState(state);
                return true;
            }
        }

        /// <summary>Attempts to enter the state registered with the specified `key` and returns it if successful.</summary>
        /// <remarks>
        /// This method does not check if the `key` is already the <see cref="CurrentKey"/>. To do so, use
        /// <see cref="TrySetState(TKey)"/> instead.
        /// </remarks>
        public TState TryResetState(TKey key)
        {
            if (Dictionary.TryGetValue(key, out var state) &&
                TryResetState(key, state))
                return state;
            else
                return null;
        }

        /// <inheritdoc/>
        object IKeyedStateMachine<TKey>.TryResetState(TKey key) => TryResetState(key);

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="IState.OnExitState"/> on the <see cref="StateMachine{TState}.CurrentState"/> then changes
        /// to the specified `key` and `state` and calls <see cref="IState.OnEnterState"/> on it.
        /// </summary>
        /// <remarks>
        /// This method does not check <see cref="IState.CanExitState"/> or <see cref="IState.CanEnterState"/>. To do
        /// that, you should use <see cref="TrySetState(TKey, TState)"/> instead.
        /// </remarks>
        public void ForceSetState(TKey key, TState state)
        {
            using (new KeyChange<TKey>(this, _CurrentKey, key))
            {
                _CurrentKey = key;
                ForceSetState(state);
            }
        }

        /// <summary>
        /// Uses <see cref="ForceSetState(TKey, TState)"/> to change to the state registered with the `key`. If nothing
        /// is registered, it use <c>null</c> and will throw an exception unless
        /// <see cref="StateMachine{TState}.AllowNullStates"/> is enabled.
        /// </summary>
        public TState ForceSetState(TKey key)
        {
            Dictionary.TryGetValue(key, out var state);
            ForceSetState(key, state);
            return state;
        }

        /// <inheritdoc/>
        object IKeyedStateMachine<TKey>.ForceSetState(TKey key) => ForceSetState(key);

        /************************************************************************************************************************/
        #region Dictionary Wrappers
        /************************************************************************************************************************/

        /// <summary>The state registered with the `key` in the <see cref="Dictionary"/>.</summary>
        public TState this[TKey key] { get => Dictionary[key]; set => Dictionary[key] = value; }

        /// <summary>Gets the state registered with the specified `key` in the <see cref="Dictionary"/>.</summary>
        public bool TryGetValue(TKey key, out TState state) => Dictionary.TryGetValue(key, out state);

        /************************************************************************************************************************/

        /// <summary>Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="Dictionary"/>.</summary>
        public ICollection<TKey> Keys => Dictionary.Keys;

        /// <summary>Gets an <see cref="ICollection{T}"/> containing the state of the <see cref="Dictionary"/>.</summary>
        public ICollection<TState> Values => Dictionary.Values;

        /************************************************************************************************************************/

        /// <summary>Gets the number of states contained in the <see cref="Dictionary"/>.</summary>
        public int Count => Dictionary.Count;

        /************************************************************************************************************************/

        /// <summary>Adds a state to the <see cref="Dictionary"/>.</summary>
        public void Add(TKey key, TState state) => Dictionary.Add(key, state);

        /// <summary>Adds a state to the <see cref="Dictionary"/>.</summary>
        public void Add(KeyValuePair<TKey, TState> item) => Dictionary.Add(item);

        /************************************************************************************************************************/

        /// <summary>Removes a state from the <see cref="Dictionary"/>.</summary>
        public bool Remove(TKey key) => Dictionary.Remove(key);

        /// <summary>Removes a state from the <see cref="Dictionary"/>.</summary>
        public bool Remove(KeyValuePair<TKey, TState> item) => Dictionary.Remove(item);

        /************************************************************************************************************************/

        /// <summary>Removes all state from the <see cref="Dictionary"/>.</summary>
        public void Clear() => Dictionary.Clear();

        /************************************************************************************************************************/

        /// <summary>Determines whether the <see cref="Dictionary"/> contains a specific value.</summary>
        public bool Contains(KeyValuePair<TKey, TState> item) => Dictionary.Contains(item);

        /// <summary>Determines whether the <see cref="Dictionary"/> contains a state with the specified `key`.</summary>
        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

        /************************************************************************************************************************/

        /// <summary>Returns an enumerator that iterates through the <see cref="Dictionary"/>.</summary>
        public IEnumerator<KeyValuePair<TKey, TState>> GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>Returns an enumerator that iterates through the <see cref="Dictionary"/>.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /************************************************************************************************************************/

        /// <summary>Copies the contents of the <see cref="Dictionary"/> to the `array` starting at the `arrayIndex`.</summary>
        public void CopyTo(KeyValuePair<TKey, TState>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);

        /************************************************************************************************************************/

        /// <summary>Indicates whether the <see cref="Dictionary"/> is read-only.</summary>
        bool ICollection<KeyValuePair<TKey, TState>>.IsReadOnly => Dictionary.IsReadOnly;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>Returns the state registered with the specified `key`, or null if none is present.</summary>
        public TState GetState(TKey key)
        {
            TryGetValue(key, out var state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Adds the specified `keys` and `states`. Both arrays must be the same size.</summary>
        public void AddRange(TKey[] keys, TState[] states)
        {
            Debug.Assert(keys.Length == states.Length,
                $"The '{nameof(keys)}' and '{nameof(states)}' arrays must be the same size.");

            for (int i = 0; i < keys.Length; i++)
            {
                Dictionary.Add(keys[i], states[i]);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the <see cref="CurrentKey"/> without changing the <see cref="StateMachine{TState}.CurrentState"/>.
        /// </summary>
        public void SetFakeKey(TKey key) => _CurrentKey = key;

        /************************************************************************************************************************/

        /// <summary>
        /// Returns a string describing the type of this state machine and its <see cref="CurrentKey"/> and
        /// <see cref="StateMachine{TState}.CurrentState"/>.
        /// </summary>
        public override string ToString()
            => $"{GetType().FullName} -> {_CurrentKey} -> {(CurrentState != null ? CurrentState.ToString() : "null")}";

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int GUILineCount => 2;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI(ref Rect area)
        {
            area.height = UnityEditor.EditorGUIUtility.singleLineHeight;

            UnityEditor.EditorGUI.BeginChangeCheck();

            var key = StateMachineUtilities.DoGenericField(area, "Current Key", _CurrentKey);

            if (UnityEditor.EditorGUI.EndChangeCheck())
                SetFakeKey(key);

            StateMachineUtilities.NextVerticalArea(ref area);

            base.DoGUI(ref area);
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
