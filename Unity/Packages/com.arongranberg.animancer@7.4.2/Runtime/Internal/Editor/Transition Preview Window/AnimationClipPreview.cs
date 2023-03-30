// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0618 // Type or member is obsolete.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A minimal <see cref="ITransitionDetailed"/> to preview an <see cref="AnimationClip"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimationClipPreview
    /// 
    [HelpURL(Strings.DocsURLs.APIDocumentation + "." + nameof(Editor) + "/" + nameof(AnimationClipPreview))]
    internal class AnimationClipPreview : ScriptableObject
    {
        /************************************************************************************************************************/

        [SerializeField]
        private Transition _Transition;

        /************************************************************************************************************************/

        [Serializable]
        [Obsolete("Only intended for internal use.")]// Prevent this type from showing up in [SerializeReference] fields.
        private class Transition : ITransitionDetailed, IAnimationClipCollection
        {
            /************************************************************************************************************************/

            [SerializeField]
            private AnimationClip _Clip;
            public ref AnimationClip Clip => ref _Clip;

            /************************************************************************************************************************/

            public object Key => _Clip;
            public float FadeDuration => 0;
            public FadeMode FadeMode => default;
            public AnimancerState CreateState() => new ClipState(_Clip);
            public void Apply(AnimancerState state) { }

            public bool IsValid => _Clip != null;
            public bool IsLooping => _Clip.isLooping;
            public float NormalizedStartTime { get => float.NaN; set => throw new NotSupportedException(); }
            public float MaximumDuration => _Clip.length;
            public float Speed { get => 1; set => throw new NotSupportedException(); }

            /************************************************************************************************************************/

            public void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.Add(_Clip);

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Preview")]
        private static void Preview(MenuCommand command)
        {
            var preview = FindObjectOfType<AnimationClipPreview>();
            if (preview == null)
            {
                preview = CreateInstance<AnimationClipPreview>();
                preview.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            }

            preview._Transition = new Transition
            {
                Clip = (AnimationClip)command.context
            };

            var serializedObject = new SerializedObject(preview);
            var property = serializedObject.FindProperty(nameof(_Transition));

            TransitionPreviewWindow.OpenOrClose(property);
        }

        /************************************************************************************************************************/
    }
}

#endif

