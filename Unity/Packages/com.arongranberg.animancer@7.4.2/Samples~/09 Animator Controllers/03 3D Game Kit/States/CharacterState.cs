// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using UnityEngine;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>
    /// Base class for the various states a <see cref="Brains.Character"/> can be in and actions they can perform.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit">3D Game Kit</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/CharacterState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Character State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(CharacterState))]
    public abstract class CharacterState : StateBehaviour, IOwnedState<CharacterState>
    {
        /************************************************************************************************************************/

        [System.Serializable]
        public class StateMachine : StateMachine<CharacterState>.WithDefault
        {
            /************************************************************************************************************************/

            [SerializeField]
            private CharacterState _Locomotion;
            public CharacterState Locomotion => _Locomotion;

            [SerializeField]
            private CharacterState _Airborne;
            public CharacterState Airborne => _Airborne;

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        [SerializeField]
        private Character _Character;
        public Character Character => _Character;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            gameObject.GetComponentInParentOrChildren(ref _Character);
        }
#endif

        /************************************************************************************************************************/

        public StateMachine<CharacterState> OwnerStateMachine => _Character.StateMachine;

        /************************************************************************************************************************/

        /// <summary>
        /// Jumping enters the <see cref="AirborneState"/>, but <see cref="CharacterController.isGrounded"/> doesn't
        /// become false until after the first update, so we want to make sure the <see cref="Character"/> won't stick
        /// to the ground during that update.
        /// </summary>
        public virtual bool StickToGround => true;

        /// <summary>
        /// Some states (such as <see cref="AirborneState"/>) will want to apply their own source of root motion, but
        /// most will just use the root motion from the animations.
        /// </summary>
        public virtual Vector3 RootMotion => _Character.Animancer.Animator.deltaPosition;

        /// <summary>
        /// Indicates whether the root motion applied each frame while this state is active should be constrained to
        /// only move in the specified <see cref="CharacterBrain.Movement"/>. Otherwise the root motion can
        /// move the <see cref="Character"/> in any direction. Default is true.
        /// </summary>
        public virtual bool FullMovementControl => true;

        /************************************************************************************************************************/
    }
}
