// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using UnityEngine;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>A <see cref="CharacterState"/> which plays a "getting hit" animation.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit/flinch">3D Game Kit/Flinch</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/FlinchState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Flinch State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(FlinchState))]
    public sealed class FlinchState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private MixerTransition2D _Animation;
        [SerializeField] private LayerMask _EnemyLayers;
        [SerializeField, Meters] private float _EnemyCheckRadius = 1;

        /************************************************************************************************************************/

        private void Awake()
        {
            _Animation.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;
        }

        /************************************************************************************************************************/

        public void OnDamageReceived() => Character.StateMachine.ForceSetState(this);

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Character.Parameters.ForwardSpeed = 0;
            Character.Animancer.Play(_Animation);

            var direction = DetermineHitDirection();

            // Once we know which direction the hit came from, we need to convert it to be relative to the model.
            // The Parameter X represents left/right so we project the direction onto the right vector.
            // The Parameter Y represents forward/back so we project the direction onto the forward vector.
            _Animation.State.Parameter = new Vector2(
                Vector3.Dot(Character.Animancer.transform.right, direction),
                Vector3.Dot(Character.Animancer.transform.forward, direction));
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Since Animancer does not actually depend on the 3D Game Kit (except for this example), we cannot reference
        /// any of its scripts from here so we cannot use their <c>IMessageReceiver</c> system which informs the
        /// defending character where the incoming hit came from.
        /// <para></para>
        /// So instead we just find the closest enemy and use that as the direction.
        /// </summary>
        private Vector3 DetermineHitDirection()
        {
            var position = Character.transform.position;
            var closestEnemySquaredDistance = float.PositiveInfinity;
            var closestEnemyDirection = default(Vector3);

            var enemies = Physics.OverlapSphere(position, _EnemyCheckRadius, _EnemyLayers);
            for (int i = 0; i < enemies.Length; i++)
            {
                var direction = enemies[i].transform.position - position;
                var squaredDistance = direction.magnitude;
                if (closestEnemySquaredDistance > squaredDistance)
                {
                    closestEnemySquaredDistance = squaredDistance;
                    closestEnemyDirection = direction;
                }
            }

            return closestEnemyDirection.normalized;
        }

        /************************************************************************************************************************/

        public override bool FullMovementControl => false;

        /************************************************************************************************************************/

        public override bool CanExitState => false;

        /************************************************************************************************************************/
    }
}
