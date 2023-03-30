// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// A variant of <see cref="IAnimationClipSource"/> which uses a <see cref="ICollection{T}"/> instead of a
    /// <see cref="List{T}"/> so that it can take a <see cref="HashSet{T}"/> to efficiently avoid adding duplicates.
    /// <see cref="AnimancerUtilities"/> contains various extension methods for this purpose.
    /// </summary>
    /// <remarks>
    /// <see cref="IAnimationClipSource"/> still needs to be the main point of entry for the Animation Window, so this
    /// interface is only used internally.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IAnimationClipCollection
    /// 
    public interface IAnimationClipCollection
    {
        /************************************************************************************************************************/

        /// <summary>Adds all the animations associated with this object to the `clips`.</summary>
        void GatherAnimationClips(ICollection<AnimationClip> clips);

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Adds the `clip` to the `clips` if it wasn't there already.
        /// </summary>
        public static void Gather(this ICollection<AnimationClip> clips, AnimationClip clip)
        {
            if (clip != null && !clips.Contains(clip))
                clips.Add(clip);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each of the `newClips`.
        /// </summary>
        public static void Gather(this ICollection<AnimationClip> clips, IList<AnimationClip> gatherFrom)
        {
            if (gatherFrom == null)
                return;

            for (int i = gatherFrom.Count - 1; i >= 0; i--)
                clips.Gather(gatherFrom[i]);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each of the `newClips`.
        /// </summary>
        public static void Gather(this ICollection<AnimationClip> clips, IEnumerable<AnimationClip> gatherFrom)
        {
            if (gatherFrom == null)
                return;

            foreach (var clip in gatherFrom)
                clips.Gather(clip);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each clip in the `asset`.
        /// </summary>
        public static void GatherFromAsset(this ICollection<AnimationClip> clips, PlayableAsset asset)
        {
            if (asset == null)
                return;

            // We want to get the tracks out of a TimelineAsset without actually referencing that class directly
            // because it comes from an optional package and Animancer does not need to depend on that package.

            var method = asset.GetType().GetMethod("GetRootTracks");
            if (method != null &&
                typeof(IEnumerable).IsAssignableFrom(method.ReturnType) &&
                method.GetParameters().Length == 0)
            {
                var rootTracks = method.Invoke(asset, null);
                GatherFromTracks(clips, rootTracks as IEnumerable);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gathers all the animations in the `tracks`.</summary>
        private static void GatherFromTracks(ICollection<AnimationClip> clips, IEnumerable tracks)
        {
            if (tracks == null)
                return;

            foreach (var track in tracks)
            {
                if (track == null)
                    continue;

                var trackType = track.GetType();

                var getClips = trackType.GetMethod("GetClips");
                if (getClips != null &&
                    typeof(IEnumerable).IsAssignableFrom(getClips.ReturnType) &&
                    getClips.GetParameters().Length == 0)
                {
                    var trackClips = getClips.Invoke(track, null) as IEnumerable;
                    if (trackClips != null)
                    {
                        foreach (var clip in trackClips)
                        {
                            var animationClip = clip.GetType().GetProperty("animationClip");
                            if (animationClip != null &&
                                animationClip.PropertyType == typeof(AnimationClip))
                            {
                                var getClip = animationClip.GetGetMethod();
                                clips.Gather(getClip.Invoke(clip, null) as AnimationClip);
                            }
                        }
                    }
                }

                var getChildTracks = trackType.GetMethod("GetChildTracks");
                if (getChildTracks != null &&
                    typeof(IEnumerable).IsAssignableFrom(getChildTracks.ReturnType) &&
                    getChildTracks.GetParameters().Length == 0)
                {
                    var childTracks = getChildTracks.Invoke(track, null);
                    GatherFromTracks(clips, childTracks as IEnumerable);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each clip gathered by
        /// <see cref="IAnimationClipSource.GetAnimationClips"/>.
        /// </summary>
        public static void GatherFromSource(this ICollection<AnimationClip> clips, IAnimationClipSource source)
        {
            if (source == null)
                return;

            var list = ObjectPool.AcquireList<AnimationClip>();
            source.GetAnimationClips(list);
            clips.Gather(list);
            ObjectPool.Release(list);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="GatherFromSource(ICollection{AnimationClip}, object)"/> for each item in the `source`.
        /// </summary>
        public static void GatherFromSource(this ICollection<AnimationClip> clips, IEnumerable source)
        {
            if (source != null)
                foreach (var item in source)
                    clips.GatherFromSource(item);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each clip in the `source`,
        /// supporting both <see cref="IAnimationClipSource"/> and <see cref="IAnimationClipCollection"/>.
        /// </summary>
        public static bool GatherFromSource(this ICollection<AnimationClip> clips, object source)
        {
            if (TryGetWrappedObject(source, out AnimationClip clip))
            {
                clips.Gather(clip);
                return true;
            }

            if (TryGetWrappedObject(source, out IAnimationClipCollection collectionSource))
            {
                collectionSource.GatherAnimationClips(clips);
                return true;
            }

            if (TryGetWrappedObject(source, out IAnimationClipSource listSource))
            {
                clips.GatherFromSource(listSource);
                return true;
            }

            if (TryGetWrappedObject(source, out IEnumerable enumerable))
            {
                clips.GatherFromSource(enumerable);
                return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Attempts to get the <see cref="AnimationClip.frameRate"/> from the `clipSource` and returns true if
        /// successful. If it has multiple animations with different rates, this method returns false.
        /// </summary>
        public static bool TryGetFrameRate(object clipSource, out float frameRate)
        {
            using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clips))
            {
                clips.GatherFromSource(clipSource);
                if (clips.Count == 0)
                {
                    frameRate = float.NaN;
                    return false;
                }

                frameRate = float.NaN;

                foreach (var clip in clips)
                {
                    if (float.IsNaN(frameRate))
                    {
                        frameRate = clip.frameRate;
                    }
                    else if (frameRate != clip.frameRate)
                    {
                        frameRate = float.NaN;
                        return false;
                    }
                }

                return frameRate > 0;
            }
        }

        /************************************************************************************************************************/
    }
}

