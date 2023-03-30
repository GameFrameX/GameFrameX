// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;

namespace Animancer.FSM
{
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateMachine_2
    partial class StateMachine<TKey, TState>
    {
        /// <summary>A <see cref="StateMachine{TKey, TState}"/> with a <see cref="DefaultKey"/>.</summary>
        /// <remarks>
        /// See <see cref="InitializeAfterDeserialize"/> if using this class in a serialized field.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/changing-states#default-states">Default States</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer.FSM/WithDefault
        /// 
        [Serializable]
        public new class WithDefault : StateMachine<TKey, TState>
        {
            /************************************************************************************************************************/

            [SerializeField]
            private TKey _DefaultKey;

            /// <summary>The starting state and main state to return to when nothing else is active.</summary>
            /// <remarks>
            /// If the <see cref="CurrentState"/> is <c>null</c> when setting this value, it calls
            /// <see cref="ForceSetState(TKey)"/> to enter the specified state immediately.
            /// <para></para>
            /// For a character, this would typically be their <em>Idle</em> state.
            /// </remarks>
            public TKey DefaultKey
            {
                get => _DefaultKey;
                set
                {
                    _DefaultKey = value;
                    if (CurrentState == null && value != null)
                        ForceSetState(value);
                }
            }

            /************************************************************************************************************************/

            /// <summary>Calls <see cref="ForceSetState(TKey)"/> with the <see cref="DefaultKey"/>.</summary>
            /// <remarks>This delegate is cached to avoid allocating garbage when used in Animancer Events.</remarks>
            public readonly Action ForceSetDefaultState;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="WithDefault"/>.</summary>
            public WithDefault()
            {
                // Silly C# doesn't allow instance delegates to be assigned using field initializers.
                ForceSetDefaultState = () => ForceSetState(_DefaultKey);
            }

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="WithDefault"/> and sets the <see cref="DefaultKey"/>.</summary>
            public WithDefault(TKey defaultKey)
                : this()
            {
                _DefaultKey = defaultKey;
                ForceSetState(defaultKey);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void InitializeAfterDeserialize()
            {
                if (CurrentState != null)
                {
                    using (new KeyChange<TKey>(this, default, _DefaultKey))
                    using (new StateChange<TState>(this, null, CurrentState))
                        CurrentState.OnEnterState();
                }
                else
                {
                    ForceSetState(_DefaultKey);
                }

                // Don't call the base method.
            }

            /************************************************************************************************************************/

            /// <summary>Attempts to enter the <see cref="DefaultKey"/> and returns true if successful.</summary>
            /// <remarks>
            /// This method returns true immediately if the specified <see cref="DefaultKey"/> is already the
            /// <see cref="CurrentKey"/>. To allow directly re-entering the same state, use
            /// <see cref="TryResetDefaultState"/> instead.
            /// </remarks>
            public TState TrySetDefaultState() => TrySetState(_DefaultKey);

            /************************************************************************************************************************/

            /// <summary>Attempts to enter the <see cref="DefaultKey"/> and returns true if successful.</summary>
            /// <remarks>
            /// This method does not check if the <see cref="DefaultKey"/> is already the <see cref="CurrentKey"/>.
            /// To do so, use <see cref="TrySetDefaultState"/> instead.
            /// </remarks>
            public TState TryResetDefaultState() => TryResetState(_DefaultKey);

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

                var state = StateMachineUtilities.DoGenericField(area, "Default Key", DefaultKey);

                if (UnityEditor.EditorGUI.EndChangeCheck())
                    DefaultKey = state;

                StateMachineUtilities.NextVerticalArea(ref area);

                base.DoGUI(ref area);
            }

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }
    }
}
