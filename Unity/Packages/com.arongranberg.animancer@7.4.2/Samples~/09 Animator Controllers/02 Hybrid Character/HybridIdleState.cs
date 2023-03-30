// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using Animancer.Examples.StateMachines;
using UnityEngine;

namespace Animancer.Examples.AnimatorControllers
{
    /// <summary>A <see cref="CharacterState"/> which plays an animation.</summary>
    /// 
    /// <remarks>
    /// This class is very similar to <see cref="IdleState"/>, except that it plays the animation inside an Animator
    /// Controller instead of as a Transition.
    /// </remarks>
    /// 
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/character">Hybrid Character</see></example>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers/HybridIdleState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Hybrid - Idle State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "/" + nameof(HybridIdleState))]
    public sealed class HybridIdleState : CharacterState
    {
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
            HybridAnimancer.SetBool(Animations.IsMoving, false);
        }

        /************************************************************************************************************************/
    }
}
