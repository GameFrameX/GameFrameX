// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Animancer Transition", order = Strings.AssetMenuOrder + 0)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(AnimancerTransitionAsset))]
    public class AnimancerTransitionAsset : AnimancerTransitionAsset<ITransition>
    {
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void Reset()
        {
            Transition = new ClipTransition();
        }
#endif

        /************************************************************************************************************************/
    }
}
