// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

namespace Animancer.FSM
{
    public partial class StateMachine<TKey, TState>
    {
        /// <summary>
        /// A simple system that can <see cref="StateMachine{TState}.InputBuffer{TStateMachine}.State"/> a state then
        /// try to enter it every time <see cref="StateMachine{TState}.InputBuffer{TStateMachine}.Update(float)"/> is
        /// called until the <see cref="StateMachine{TState}.InputBuffer{TStateMachine}.TimeOut"/> expires.
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
        public new class InputBuffer : InputBuffer<StateMachine<TKey, TState>>
        {
            /************************************************************************************************************************/

            /// <summary>The <typeparamref name="TKey"/> of the state this buffer is currently attempting to enter.</summary>
            public TKey Key { get; set; }

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="InputBuffer"/>.</summary>
            public InputBuffer() { }

            /// <summary>Creates a new <see cref="InputBuffer"/> for the specified `stateMachine`.</summary>
            public InputBuffer(StateMachine<TKey, TState> stateMachine) : base(stateMachine) { }

            /************************************************************************************************************************/

            /// <summary>
            /// If a state is registered with the `key`, this method calls <see cref="Buffer(TKey, TState, float)"/>
            /// and returns true. Otherwise it returns false.
            /// </summary>
            /// <remarks>Doesn't actually attempt to enter the state until <see cref="Update(float)"/> is called.</remarks>
            public bool Buffer(TKey key, float timeOut)
            {
                if (StateMachine.TryGetValue(key, out var state))
                {
                    Buffer(key, state, timeOut);
                    return true;
                }
                else return false;
            }

            /// <summary>
            /// Sets the <see cref="Key"/>, <see cref="StateMachine{TState}.InputBuffer.State"/>, and
            /// <see cref="TimeOut"/>.
            /// </summary>
            /// <remarks>Doesn't actually attempt to enter the state until <see cref="Update(float)"/> is called.</remarks>
            public void Buffer(TKey key, TState state, float timeOut)
            {
                Key = key;
                Buffer(state, timeOut);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override bool TryEnterState()
                => StateMachine.TryResetState(Key, State);

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Clear()
            {
                base.Clear();
                Key = default;
            }

            /************************************************************************************************************************/
        }
    }
}
