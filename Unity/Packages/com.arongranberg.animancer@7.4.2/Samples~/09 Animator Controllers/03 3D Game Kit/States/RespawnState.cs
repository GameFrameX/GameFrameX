// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;
using UnityEngine.Events;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>
    /// A <see cref="CharacterState"/> which teleports back to the starting position, plays an animation then returns
    /// to the <see cref="Character.Idle"/> state.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit/respawn">3D Game Kit/Respawn</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/RespawnState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Respawn State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(RespawnState))]
    public sealed class RespawnState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private ClipTransition _Animation;
        [SerializeField] private UnityEvent _OnEnterState;// See the Read Me.
        [SerializeField] private UnityEvent _OnExitState;// See the Read Me.

        private Vector3 _StartingPosition;

        /************************************************************************************************************************/

        private void Awake()
        {
            _Animation.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;
            _StartingPosition = transform.position;
        }

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Character.Animancer.Play(_Animation);
            Character.transform.position = _StartingPosition;
            _OnEnterState.Invoke();
        }

        /************************************************************************************************************************/

        private void OnDisable()
        {
            _OnExitState.Invoke();
        }

        /************************************************************************************************************************/

        public override bool CanExitState => false;

        /************************************************************************************************************************/
    }
}
