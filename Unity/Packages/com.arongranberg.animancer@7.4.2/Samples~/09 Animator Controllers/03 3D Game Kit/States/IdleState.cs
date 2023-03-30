// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using System;
using UnityEngine;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>
    /// A <see cref="CharacterState"/> which keeps the character standing still and occasionally plays alternate
    /// animations if it remains active for long enough.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit/idle">3D Game Kit/Idle</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/IdleState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Idle State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(IdleState))]
    public sealed class IdleState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private ClipTransition _MainAnimation;
        [SerializeField, Seconds] private float _FirstRandomizeDelay = 5;
        [SerializeField, Seconds] private float _MinRandomizeInterval = 0;
        [SerializeField, Seconds] private float _MaxRandomizeInterval = 20;
        [SerializeField] private ClipTransition[] _RandomAnimations;

        private float _RandomizeTime;

        // _RandomizeDelay was originally handled by the PlayerController (Idle Timeout).
        // The min and max interval were handled by the RandomStateSMB on the Idle state in IdleSM.

        /************************************************************************************************************************/

        private void Awake()
        {
            Action onEnd = PlayMainAnimation;
            for (int i = 0; i < _RandomAnimations.Length; i++)
            {
                _RandomAnimations[i].Events.OnEnd = onEnd;

                // We could just do `...OnEnd = PlayMainAnimation` instead of declaring the delegate first, but that
                // assignment is actually shorthand for `new Action(PlayMainAnimation)` which would create a new
                // delegate object for each animation. This way all animations just share the same object.
            }
        }

        /************************************************************************************************************************/

        public override bool CanEnterState => Character.Movement.IsGrounded;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            PlayMainAnimation();
            _RandomizeTime += _FirstRandomizeDelay;
        }

        private void PlayMainAnimation()
        {
            _RandomizeTime = UnityEngine.Random.Range(_MinRandomizeInterval, _MaxRandomizeInterval);
            Character.Animancer.Play(_MainAnimation);
        }

        /************************************************************************************************************************/

        private void FixedUpdate()
        {
            if (Character.CheckMotionState())
                return;

            Character.Movement.UpdateSpeedControl();

            // We use time where Mecanim used normalized time because choosing a number of seconds is much simpler than
            // finding out how long the animation is and working with multiples of that value.
            var state = Character.Animancer.States.Current;
            if (state == _MainAnimation.State &&
                state.Time >= _RandomizeTime)
            {
                PlayRandomAnimation();
            }
        }

        /************************************************************************************************************************/

        private void PlayRandomAnimation()
        {
            var index = UnityEngine.Random.Range(0, _RandomAnimations.Length);
            var animation = _RandomAnimations[index];
            Character.Animancer.Play(animation);
            CustomFade.Apply(Character.Animancer, Easing.Function.SineInOut);
        }

        /************************************************************************************************************************/
    }
}
