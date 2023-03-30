// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> based <see cref="ITransition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/assets">Transition Assets</see>
    /// <para></para>
    /// When adding a <see cref="CreateAssetMenuAttribute"/> to any derived classes, you can use
    /// <see cref="Strings.MenuPrefix"/> and <see cref="Strings.AssetMenuOrder"/>.
    /// <para></para>
    /// If you are using <see cref="AnimancerEvent"/>s, consider using an <see cref="UnShared{TAsset}"/> instead of
    /// referencing this asset directly in order to avoid common issues with shared events.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerTransitionAssetBase
    /// 
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(AnimancerTransitionAssetBase))]
    public abstract partial class AnimancerTransitionAssetBase : ScriptableObject, ITransition, IWrapper, IAnimationClipSource
    {
        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="ITransition"/> wrapped by this <see cref="ScriptableObject"/>.</summary>
        public abstract ITransition GetTransition();

        /// <inheritdoc/>
        object IWrapper.WrappedObject => GetTransition();

        /************************************************************************************************************************/

        /// <summary>Can this transition create a valid <see cref="AnimancerState"/>?</summary>
        public virtual bool IsValid => GetTransition().IsValid();

        /// <inheritdoc/>
        public virtual float FadeDuration => GetTransition().FadeDuration;

        /// <inheritdoc/>
        public virtual object Key => GetTransition().Key;

        /// <inheritdoc/>
        public virtual FadeMode FadeMode => GetTransition().FadeMode;

        /// <inheritdoc/>
        public virtual AnimancerState CreateState() => GetTransition().CreateState();

        /// <inheritdoc/>
        public virtual void Apply(AnimancerState state)
        {
            GetTransition().Apply(state);
            state.SetDebugName(name);
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipSource"/>]
        /// Calls <see cref="AnimancerUtilities.GatherFromSource(ICollection{AnimationClip}, object)"/>.
        /// </summary>
        public virtual void GetAnimationClips(List<AnimationClip> clips)
            => clips.GatherFromSource(GetTransition());

        /************************************************************************************************************************/
    }
}

/************************************************************************************************************************/

#if UNITY_EDITOR
namespace Animancer.Editor
{
    /// <summary>A custom editor for <see cref="AnimancerTransitionAssetBase"/>.</summary>
    [CustomEditor(typeof(AnimancerTransitionAssetBase), true), CanEditMultipleObjects]
    internal class AnimancerTransitionAssetBaseEditor : ScriptableObjectEditor { }
}
#endif
