// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using Animancer.Examples.StateMachines;
using Animancer.Units;
using UnityEngine;

namespace Animancer.Examples.AnimatorControllers
{
    /// <summary>
    /// A <see cref="CharacterState"/> which moves the character according to their
    /// <see cref="CharacterParameters.MovementDirection"/>.
    /// </summary>
    /// 
    /// <remarks>
    /// This class is very similar to <see cref="MoveState"/>, except that it manages a
    /// Blend Tree instead of a Mixer.
    /// </remarks>
    /// 
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/character">Hybrid Character</see></example>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers/HybridMoveState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Hybrid - Move State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "/" + nameof(HybridMoveState))]
    public sealed class HybridMoveState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField, DegreesPerSecond] private float _TurnSpeed = 360;
        [SerializeField] private float _ParameterFadeSpeed = 2;

        private float _MoveBlend;

        /************************************************************************************************************************/

        /// <summary>
        /// Normally the <see cref="Character"/> class would have a reference to the specific type of
        /// <see cref="AnimancerComponent"/> we want, but for the sake of reusing code from the earlier example, we
        /// just use a type cast here.
        /// </summary>
        private HybridAnimancerComponent HybridAnimancer
            => (HybridAnimancerComponent)Character.Animancer;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            HybridAnimancer.PlayController();
            HybridAnimancer.SetBool(Animations.IsMoving, true);
            _MoveBlend = Character.Parameters.WantsToRun ? 1 : 0;
        }

        /************************************************************************************************************************/

        private void Update()
        {
            UpdateAnimation();
            UpdateTurning();
        }

        /************************************************************************************************************************/

        /// <summary>This method is similar to <see cref="MoveState.UpdateAnimation"/>.</summary>
        private void UpdateAnimation()
        {
            var target = Character.Parameters.WantsToRun ? 1 : 0;
            _MoveBlend = Mathf.MoveTowards(
                _MoveBlend,
                target,
                _ParameterFadeSpeed * Time.deltaTime);
            HybridAnimancer.SetFloat(Animations.MoveBlend, _MoveBlend);
        }

        /************************************************************************************************************************/

        /// <remarks>This method is identical to <see cref="MoveState.UpdateTurning"/>.</remarks>
        private void UpdateTurning()
        {
            var movement = Character.Parameters.MovementDirection;
            if (movement == default)
                return;

            var targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            var turnDelta = _TurnSpeed * Time.deltaTime;

            var transform = Character.Animancer.transform;
            var eulerAngles = transform.eulerAngles;
            eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, targetAngle, turnDelta);
            transform.eulerAngles = eulerAngles;
        }

        /************************************************************************************************************************/
    }
}
