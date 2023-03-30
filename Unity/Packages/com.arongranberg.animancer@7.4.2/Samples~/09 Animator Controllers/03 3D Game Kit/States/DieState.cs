// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using UnityEngine;
using UnityEngine.Events;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>A <see cref="CharacterState"/> which plays a "dying" animation.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit/die">3D Game Kit/Die</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/DieState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Die State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(DieState))]
    public sealed class DieState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private ClipTransition _Animation;
        [SerializeField] private CharacterState _RespawnState;
        [SerializeField] private UnityEvent _OnEnterState;// See the Read Me.
        [SerializeField] private UnityEvent _OnExitState;// See the Read Me.

        /************************************************************************************************************************/

        private void Awake()
        {
            // Respawn immediately when the animation ends.
            _Animation.Events.OnEnd = _RespawnState.ForceEnterState;
        }

        /************************************************************************************************************************/

        public void OnDeath()
        {
            Character.StateMachine.ForceSetState(this);
        }

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Character.Animancer.Play(_Animation);
            Character.Parameters.ForwardSpeed = 0;
            _OnEnterState.Invoke();
        }

        /************************************************************************************************************************/

        private void OnDisable()
        {
            _OnExitState.Invoke();
        }

        /************************************************************************************************************************/

        public override bool FullMovementControl => false;

        /************************************************************************************************************************/

        public override bool CanExitState => false;

        /************************************************************************************************************************/
    }
}
