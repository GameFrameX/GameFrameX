// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer.FSM
{
    public partial class StateMachine<TState>
    {
        /// <summary>
        /// A simple system that can <see cref="InputBuffer{TStateMachine}.State"/> a state then try to enter it every time
        /// <see cref="InputBuffer{TStateMachine}.Update(float)"/> is called until the
        /// <see cref="InputBuffer{TStateMachine}.TimeOut"/> expires.
        /// </summary>
        /// 
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/utilities#input-buffers">Input Buffers</see>
        /// </remarks>
        /// 
        /// <example>See <see cref="StateMachine{TState}.InputBuffer{TStateMachine}"/>.</example>
        /// 
        /// https://kybernetik.com.au/animancer/api/Animancer.FSM/InputBuffer
        /// 
        public class InputBuffer : InputBuffer<StateMachine<TState>>
        {
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="InputBuffer"/>.</summary>
            public InputBuffer() { }

            /// <summary>Creates a new <see cref="InputBuffer"/> for the specified `stateMachine`.</summary>
            public InputBuffer(StateMachine<TState> stateMachine) : base(stateMachine) { }

            /************************************************************************************************************************/
        }

        /// <summary>
        /// A simple system that can <see cref="Buffer"/> a state then try to enter it every time
        /// <see cref="Update(float)"/> is called until the <see cref="TimeOut"/> expires.
        /// </summary>
        /// 
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm/utilities#input-buffers">Input Buffers</see>
        /// </remarks>
        /// 
        /// <example><code>
        /// public StateMachine&lt;CharacterState&gt; stateMachine;// Initialized elsewhere.
        /// 
        /// [SerializeField] private CharacterState _Attack;
        /// [SerializeField] private float _AttackInputTimeOut = 0.5f;
        /// 
        /// private StateMachine&lt;CharacterState&gt;.InputBuffer _InputBuffer;
        /// 
        /// private void Awake()
        /// {
        ///     // Initialize the buffer.
        ///     _InputBuffer = new StateMachine&lt;CharacterState&gt;.InputBuffer(stateMachine);
        /// }
        /// 
        /// private void Update()
        /// {
        ///     // When input is detected, buffer the desired state.
        ///     if (Input.GetButtonDown("Fire1"))// Left Click by default.
        ///     {
        ///         _InputBuffer.Buffer(_Attack, _AttackInputTimeOut);
        ///     }
        /// 
        ///     // At the end of the frame, Update the buffer so it tries to enter the buffered state.
        ///     // After the time out, it will clear itself so Update does nothing until something else is buffered.
        ///     _InputBuffer.Update();
        /// }
        /// </code></example>
        /// 
        /// https://kybernetik.com.au/animancer/api/Animancer.FSM/InputBuffer_1
        /// 
        public class InputBuffer<TStateMachine> where TStateMachine : StateMachine<TState>
        {
            /************************************************************************************************************************/

            private TStateMachine _StateMachine;

            /// <summary>The <see cref="StateMachine{TState}"/> this buffer is feeding input to.</summary>
            public TStateMachine StateMachine
            {
                get => _StateMachine;
                set
                {
                    _StateMachine = value;
                    Clear();
                }
            }

            /// <summary>The <typeparamref name="TState"/> this buffer is currently attempting to enter.</summary>
            public TState State { get; set; }

            /// <summary>The amount of time left before the <see cref="State"/> is cleared.</summary>
            public float TimeOut { get; set; }

            /************************************************************************************************************************/

            /// <summary>Is this buffer currently trying to enter a <see cref="State"/>?</summary>
            public bool IsActive => State != null;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="InputBuffer{TStateMachine}"/>.</summary>
            public InputBuffer() { }

            /// <summary>Creates a new <see cref="InputBuffer{TStateMachine}"/> for the specified `stateMachine`.</summary>
            public InputBuffer(TStateMachine stateMachine) => _StateMachine = stateMachine;

            /************************************************************************************************************************/

            /// <summary>Sets the <see cref="State"/> and <see cref="TimeOut"/>.</summary>
            /// <remarks>Doesn't actually attempt to enter the state until <see cref="Update(float)"/> is called.</remarks>
            public void Buffer(TState state, float timeOut)
            {
                State = state;
                TimeOut = timeOut;
            }

            /************************************************************************************************************************/

            /// <summary>Attempts to enter the <see cref="State"/> and returns true if successful.</summary>
            protected virtual bool TryEnterState() => StateMachine.TryResetState(State);

            /************************************************************************************************************************/

            /// <summary>Calls <see cref="Update(float)"/> using <see cref="Time.deltaTime"/>.</summary>
            /// <remarks>This method should be called at the end of a frame after any calls to <see cref="Buffer"/>.</remarks>
            public bool Update() => Update(Time.deltaTime);

            /// <summary>
            /// Attempts to enter the <see cref="State"/> if there is one and returns true if successful. Otherwise the
            /// <see cref="TimeOut"/> is decreased by `deltaTime` and <see cref="Clear"/> is called if it reaches 0.
            /// </summary>
            /// <remarks>This method should be called at the end of a frame after any calls to <see cref="Buffer"/>.</remarks>
            public bool Update(float deltaTime)
            {
                if (IsActive)
                {
                    if (TryEnterState())
                    {
                        Clear();
                        return true;
                    }
                    else
                    {
                        TimeOut -= deltaTime;

                        if (TimeOut < 0)
                            Clear();
                    }
                }

                return false;
            }

            /************************************************************************************************************************/

            /// <summary>Clears this buffer so it stops trying to enter the <see cref="State"/>.</summary>
            public virtual void Clear()
            {
                State = null;
                TimeOut = default;
            }

            /************************************************************************************************************************/
        }
    }
}
