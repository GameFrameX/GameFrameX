// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer.Examples.AnimatorControllers
{
    /// <summary>A central location for cached Animator Controller hashes.</summary>
    /// 
    /// <remarks>
    /// <see href="https://kybernetik.com.au/weaver">Weaver</see> has a system for procedurally generating an
    /// <see href="https://kybernetik.com.au/weaver/docs/project-constants/animations">Animations</see> script like
    /// this so that you don't need to keep it in sync with your Animator Controllers manually.
    /// It's included in Weaver Lite for FREE.
    /// </remarks>
    /// 
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/character">Hybrid Character</see></example>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers/Animations
    /// 
    public static class Animations
    {
        /************************************************************************************************************************/

        public static readonly int IsMoving = Animator.StringToHash("IsMoving");

        public static readonly int MoveBlend = Animator.StringToHash("MoveBlend");

        /************************************************************************************************************************/
    }
}
