// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;

namespace Animancer.FSM
{
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateMachine_1
    partial class StateMachine<TState>
    {
        /// <summary>A <see cref="StateMachine{TState}"/> with a <see cref="DefaultState"/>.</summary>
        /// <remarks>
        /// See <see cref="InitializeAfterDeserialize"/> if using this class in a serialized field.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/changing-states#default-states">Default States</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer.FSM/WithDefault
        /// 
        [Serializable]
        public class WithDefault : StateMachine<TState>
        {
            /************************************************************************************************************************/

            [SerializeField]
            private TState _DefaultState;

            /// <summary>The starting state and main state to return to when nothing else is active.</summary>
            /// <remarks>
            /// If the <see cref="CurrentState"/> is <c>null</c> when setting this value, it calls
            /// <see cref="ForceSetState(TState)"/> to enter the specified state immediately.
            /// <para></para>
            /// For a character, this would typically be their <em>Idle</em> state.
            /// </remarks>
            public TState DefaultState
            {
                get => _DefaultState;
                set
                {
                    _DefaultState = value;
                    if (_CurrentState == null && value != null)
                        ForceSetState(value);
                }
            }

            /************************************************************************************************************************/

            /// <summary>Calls <see cref="ForceSetState(TState)"/> with the <see cref="DefaultState"/>.</summary>
            /// <remarks>This delegate is cached to avoid allocating garbage when used in Animancer Events.</remarks>
            public readonly Action ForceSetDefaultState;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="WithDefault"/>.</summary>
            public WithDefault()
            {
                // Silly C# doesn't allow instance delegates to be assigned using field initializers.
                ForceSetDefaultState = () => ForceSetState(_DefaultState);
            }

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="WithDefault"/> and sets the <see cref="DefaultState"/>.</summary>
            public WithDefault(TState defaultState)
                : this()
            {
                _DefaultState = defaultState;
                ForceSetState(defaultState);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void InitializeAfterDeserialize()
            {
                if (_CurrentState != null)
                {
                    using (new StateChange<TState>(this, null, _CurrentState))
                        _CurrentState.OnEnterState();
                }
                else if (_DefaultState != null)
                {
                    using (new StateChange<TState>(this, null, CurrentState))
                    {
                        _CurrentState = _DefaultState;
                        _CurrentState.OnEnterState();
                    }
                }

                // Don't call the base method.
            }

            /************************************************************************************************************************/

            /// <summary>Attempts to enter the <see cref="DefaultState"/> and returns true if successful.</summary>
            /// <remarks>
            /// This method returns true immediately if the specified <see cref="DefaultState"/> is already the
            /// <see cref="CurrentState"/>. To allow directly re-entering the same state, use
            /// <see cref="TryResetDefaultState"/> instead.
            /// </remarks>
            public bool TrySetDefaultState() => TrySetState(DefaultState);

            /************************************************************************************************************************/

            /// <summary>Attempts to enter the <see cref="DefaultState"/> and returns true if successful.</summary>
            /// <remarks>
            /// This method does not check if the <see cref="DefaultState"/> is already the <see cref="CurrentState"/>.
            /// To do so, use <see cref="TrySetDefaultState"/> instead.
            /// </remarks>
            public bool TryResetDefaultState() => TryResetState(DefaultState);

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

                var state = StateMachineUtilities.DoGenericField(area, "Default State", DefaultState);

                if (UnityEditor.EditorGUI.EndChangeCheck())
                    DefaultState = state;

                StateMachineUtilities.NextVerticalArea(ref area);

                base.DoGUI(ref area);
            }

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }
    }
}
