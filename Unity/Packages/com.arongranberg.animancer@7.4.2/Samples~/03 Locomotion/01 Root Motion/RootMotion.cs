// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using System;
using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>Demonstrates how you can use Root Motion for some animations but not others.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/locomotion/root-motion">Root Motion</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Locomotion/RootMotion
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Locomotion - Root Motion")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Locomotion) + "/" + nameof(RootMotion))]
    public sealed class RootMotion : MonoBehaviour
    {
        /************************************************************************************************************************/

        /// <summary>A <see cref="ClipTransition"/> with an <see cref="_ApplyRootMotion"/> toggle.</summary>
        [Serializable]
        public class MotionTransition : ClipTransition
        {
            /************************************************************************************************************************/

            [SerializeField, Tooltip("Should Root Motion be enabled when this animation plays?")]
            private bool _ApplyRootMotion;

            /************************************************************************************************************************/

            public override void Apply(AnimancerState state)
            {
                base.Apply(state);
                state.Root.Component.Animator.applyRootMotion = _ApplyRootMotion;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField, Meters] private float _MaxDistance;
        [SerializeField] private MotionTransition[] _Animations;

        private Vector3 _Start;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Start = transform.position;
            Play(0);
        }

        /************************************************************************************************************************/

        /// <summary>Plays the animation at the specified `index` in the <see cref="_Animations"/> array.</summary>
        /// <remarks>This method is called by UI Buttons.</remarks>
        public void Play(int index)
        {
            _Animancer.Play(_Animations[index]);
        }

        /************************************************************************************************************************/

        /// <summary>Teleports this object back to its starting location if it moves too far away.</summary>
        private void FixedUpdate()
        {
            if (Vector3.Distance(_Start, transform.position) > _MaxDistance)
                transform.position = _Start;
        }

        /************************************************************************************************************************/
    }
}
