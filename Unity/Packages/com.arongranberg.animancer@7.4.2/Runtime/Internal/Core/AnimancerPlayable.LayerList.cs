// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerPlayable
    /// 
    partial class AnimancerPlayable
    {
        /// <summary>A list of <see cref="AnimancerLayer"/>s with methods to control their mixing and masking.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">Layers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/LayerList
        /// 
        public class LayerList : IEnumerable<AnimancerLayer>, IAnimationClipCollection
        {
            /************************************************************************************************************************/
            #region Fields
            /************************************************************************************************************************/

            /// <summary>The <see cref="AnimancerPlayable"/> at the root of the graph.</summary>
            protected readonly AnimancerPlayable Root;

            /// <summary>[Internal] The layers which each manage their own set of animations.</summary>
            /// <remarks>This field should never be null so it shouldn't need null-checking.</remarks>
            private AnimancerLayer[] _Layers;

            /// <summary>The <see cref="AnimationLayerMixerPlayable"/> which blends the layers.</summary>
            protected readonly AnimationLayerMixerPlayable LayerMixer;

            /// <summary>The number of layers that have actually been created.</summary>
            private int _Count;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="LayerList"/>.</summary>
            protected LayerList(AnimancerPlayable root)
            {
                Root = root;
                _Layers = new AnimancerLayer[DefaultCapacity];
            }

            /************************************************************************************************************************/

            /// <summary>[Internal]
            /// Creates a new <see cref="LayerList"/> with an <see cref="AnimationLayerMixerPlayable"/>.
            /// </summary>
            internal LayerList(AnimancerPlayable root, out Playable layerMixer)
                : this(root)
            {
                layerMixer = LayerMixer = AnimationLayerMixerPlayable.Create(root._Graph, 1);
                Root._Graph.Connect(layerMixer, 0, Root._RootPlayable, 0);
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Sets the <see cref="Layers"/> and assigns the main <see cref="Playable"/> of this list.
            /// </summary>
            public virtual void Activate(AnimancerPlayable root)
            {
                Activate(root, LayerMixer);
            }

            /// <summary>[Pro-Only]
            /// Sets this list as the <see cref="Layers"/> and the <see cref="Playable"/> used to mix them.
            /// </summary>
            protected void Activate(AnimancerPlayable root, Playable mixer)
            {
#if UNITY_ASSERTIONS
                if (Root != root)
                    throw new ArgumentException(
                        $"{nameof(AnimancerPlayable)}.{nameof(LayerList)}.{nameof(Root)} mismatch:" +
                        $" cannot use a list in an {nameof(AnimancerPlayable)} that is not its {nameof(Root)}");
#endif

                _Layers = root.Layers._Layers;
                _Count = root.Layers._Count;

                root._RootPlayable.DisconnectInput(0);
                root.Graph.Connect(mixer, 0, root._RootPlayable, 0);
                root.Layers = this;
                root._LayerMixer = mixer;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region List Operations
            /************************************************************************************************************************/

            /// <summary>[Pro-Only] The number of layers in this list.</summary>
            /// <exception cref="ArgumentOutOfRangeException">
            /// The value is set higher than the <see cref="DefaultCapacity"/>. This is simply a safety measure,
            /// so if you do actually need more layers you can just increase the limit.
            /// </exception>
            /// <exception cref="IndexOutOfRangeException">The value is set to a negative number.</exception>
            public int Count
            {
                get => _Count;
                set
                {
                    var count = _Count;

                    if (value == count)
                        return;

                    CheckAgain:

                    if (value > count)// Increasing.
                    {
                        Add();
                        count++;
                        goto CheckAgain;
                    }
                    else// Decreasing.
                    {
                        while (value < count--)
                        {
                            var layer = _Layers[count];
                            if (layer._Playable.IsValid())
                                Root._Graph.DestroySubgraph(layer._Playable);
                            layer.DestroyStates();
                        }

                        Array.Clear(_Layers, value, _Count - value);

                        _Count = value;

                        Root._LayerMixer.SetInputCount(value);
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// If the <see cref="Count"/> is below the specified `min`, this method increases it to that value.
            /// </summary>
            public void SetMinCount(int min)
            {
                if (Count < min)
                    Count = min;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// The maximum number of layers that can be created before an <see cref="ArgumentOutOfRangeException"/> will
            /// be thrown (default 4).
            /// <para></para>
            /// Lowering this value will not affect layers that have already been created.
            /// </summary>
            /// <example>
            /// To set this value automatically when the application starts, place the following method in any class:
            /// <para></para><code>
            /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            /// private static void SetMaxLayerCount()
            /// {
            ///     Animancer.AnimancerPlayable.LayerList.DefaultCapacity = 8;
            /// }
            /// </code>
            /// Otherwise you can set the <see cref="Capacity"/> of each individual list:
            /// <para></para><code>
            /// AnimancerComponent animancer;
            /// animancer.Layers.Capacity = 8;
            /// </code></example>
            public static int DefaultCapacity { get; set; } = 4;

            /// <summary>[Pro-Only]
            /// If the <see cref="DefaultCapacity"/> is below the specified `min`, this method increases it to that value.
            /// </summary>
            public static void SetMinDefaultCapacity(int min)
            {
                if (DefaultCapacity < min)
                    DefaultCapacity = min;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// The maximum number of layers that can be created before an <see cref="ArgumentOutOfRangeException"/> will
            /// be thrown. The initial capacity is determined by <see cref="DefaultCapacity"/>.
            /// </summary>
            /// 
            /// <remarks>
            /// Lowering this value will destroy any layers beyond the specified value.
            /// <para></para>
            /// Changing this value will cause the allocation of a new array and garbage collection of the old one, so
            /// you should generally set the <see cref="DefaultCapacity"/> before initializing this list.
            /// </remarks>
            /// 
            /// <exception cref="ArgumentOutOfRangeException">The value is not greater than 0.</exception>
            public int Capacity
            {
                get => _Layers.Length;
                set
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value), $"must be greater than 0 ({value} <= 0)");

                    if (_Count > value)
                        Count = value;

                    Array.Resize(ref _Layers, value);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Creates and returns a new <see cref="AnimancerLayer"/> at the end of this list.</summary>
            /// <remarks>If the <see cref="Capacity"/> would be exceeded, it will be doubled.</remarks>
            public AnimancerLayer Add()
            {
                var index = _Count;

                if (index >= _Layers.Length)
                    Capacity *= 2;

                _Count = index + 1;
                Root._LayerMixer.SetInputCount(_Count);

                var layer = new AnimancerLayer(Root, index);
                _Layers[index] = layer;
                return layer;
            }

            /************************************************************************************************************************/

            /// <summary>Returns the layer at the specified index. If it didn't already exist, this method creates it.</summary>
            /// <remarks>To only get an existing layer without creating new ones, use <see cref="GetLayer"/> instead.</remarks>
            public AnimancerLayer this[int index]
            {
                get
                {
                    SetMinCount(index + 1);
                    return _Layers[index];
                }
            }

            /************************************************************************************************************************/

            /// <summary>Returns the layer at the specified index.</summary>
            /// <remarks>To create a new layer if the target doesn't exist, use <see cref="this[int]"/> instead.</remarks>
            public AnimancerLayer GetLayer(int index) => _Layers[index];

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Enumeration
            /************************************************************************************************************************/

            /// <summary>Returns an enumerator that will iterate through all layers.</summary>
            public FastEnumerator<AnimancerLayer> GetEnumerator()
                => new FastEnumerator<AnimancerLayer>(_Layers, _Count);

            /// <inheritdoc/>
            IEnumerator<AnimancerLayer> IEnumerable<AnimancerLayer>.GetEnumerator()
                => GetEnumerator();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimationClipCollection"/>] Gathers all the animations in all layers.</summary>
            public void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSource(_Layers);

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Layer Details
            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Is the layer at the specified index is set to additive blending?
            /// Otherwise it will override lower layers.
            /// </summary>
            public virtual bool IsAdditive(int index)
            {
                return LayerMixer.IsLayerAdditive((uint)index);
            }

            /// <summary>[Pro-Only]
            /// Sets the layer at the specified index to blend additively with earlier layers (if true) or to override them
            /// (if false). Newly created layers will override by default.
            /// </summary>
            public virtual void SetAdditive(int index, bool value)
            {
                SetMinCount(index + 1);
                LayerMixer.SetLayerAdditive((uint)index, value);
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Sets an <see cref="AvatarMask"/> to determine which bones the layer at the specified index will affect.
            /// </summary>
            public virtual void SetMask(int index, AvatarMask mask)
            {
                SetMinCount(index + 1);

#if UNITY_ASSERTIONS
                _Layers[index]._Mask = mask;
#endif

                if (mask == null)
                    mask = new AvatarMask();

                LayerMixer.SetLayerMaskFromAvatarMask((uint)index, mask);
            }

            /************************************************************************************************************************/

            /// <summary>[Editor-Conditional] Sets the Inspector display name of the layer at the specified index.</summary>
            [System.Diagnostics.Conditional(Strings.UnityEditor)]
            public void SetDebugName(int index, string name) => this[index].SetDebugName(name);

            /************************************************************************************************************************/

            /// <summary>
            /// The average velocity of the root motion of all currently playing animations, taking their current
            /// <see cref="AnimancerNode.Weight"/> into account.
            /// </summary>
            public Vector3 AverageVelocity
            {
                get
                {
                    var velocity = default(Vector3);

                    for (int i = 0; i < _Count; i++)
                    {
                        var layer = _Layers[i];
                        velocity += layer.AverageVelocity * layer.Weight;
                    }

                    return velocity;
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

