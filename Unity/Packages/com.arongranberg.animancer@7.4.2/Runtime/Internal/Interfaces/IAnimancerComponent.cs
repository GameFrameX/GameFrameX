// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>Interface for components that manage an <see cref="AnimancerPlayable"/>.</summary>
    /// <remarks>
    /// Despite the name, this interface is not necessarily limited to only <see cref="Component"/>s.
    /// <para></para>
    /// This interface allows Animancer Lite to reference an <see cref="AnimancerComponent"/> inside the pre-compiled
    /// DLL while allowing that component to remain outside as a regular script. Otherwise everything would need to be
    /// in the DLL which would cause Unity to lose all the script references when upgrading from Animancer Lite to Pro.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/component-types">Component Types</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IAnimancerComponent
    /// 
    public interface IAnimancerComponent
    {
        /************************************************************************************************************************/
#pragma warning disable IDE1006 // Naming Styles.
        /************************************************************************************************************************/

        /// <summary>Will this component be updated?</summary>
        bool enabled { get; }

        /// <summary>The <see cref="GameObject"/> this component is attached to.</summary>
        GameObject gameObject { get; }

        /************************************************************************************************************************/
#pragma warning restore IDE1006 // Naming Styles.
        /************************************************************************************************************************/

        /// <summary>The <see cref="UnityEngine.Animator"/> component which this script controls.</summary>
        Animator Animator { get; set; }

        /// <summary>The internal system which manages the playing animations.</summary>
        AnimancerPlayable Playable { get; }

        /// <summary>Has the <see cref="Playable"/> been initialized?</summary>
        bool IsPlayableInitialized { get; }

        /// <summary>Will the object be reset to its original values when disabled?</summary>
        bool ResetOnDisable { get; }

        /// <summary>
        /// Determines when animations are updated and which time source is used. This property is mainly a wrapper
        /// around the <see cref="Animator.updateMode"/>.
        /// </summary>
        AnimatorUpdateMode UpdateMode { get; set; }

        /************************************************************************************************************************/

        /// <summary>Returns the dictionary key to use for the `clip`.</summary>
        object GetKey(AnimationClip clip);

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] The name of the serialized backing field for the <see cref="Animator"/> property.</summary>
        string AnimatorFieldName { get; }

        /// <summary>[Editor-Only]
        /// The name of the serialized backing field for the <see cref="AnimancerComponent.ActionOnDisable"/> property.
        /// </summary>
        string ActionOnDisableFieldName { get; }

        /// <summary>[Editor-Only] The <see cref="UpdateMode"/> that was first used when this script initialized.</summary>
        /// <remarks>
        /// This is used to give a warning when changing to or from <see cref="AnimatorUpdateMode.AnimatePhysics"/> at
        /// runtime since it won't work correctly.
        /// </remarks>
        AnimatorUpdateMode? InitialUpdateMode { get; }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

