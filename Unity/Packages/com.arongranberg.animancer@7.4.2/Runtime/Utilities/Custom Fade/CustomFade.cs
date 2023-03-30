// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// A system which fades animation weights animations using a custom calculation rather than linear interpolation.
    /// </summary>
    /// 
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading#custom-fade">Custom Fade</see>
    /// </remarks>
    /// 
    /// <example><code>
    /// [SerializeField] private AnimancerComponent _Animancer;
    /// [SerializeField] private AnimationClip _Clip;
    /// 
    /// private void Awake()
    /// {
    ///     // Start fading the animation normally.
    ///     var state = _Animancer.Play(_Clip, 0.25f);
    ///     
    ///     // Then apply the custom fade to modify it.
    ///     CustomFade.Apply(state, Easing.Sine.InOut);// Use a delegate.
    ///     CustomFade.Apply(state, Easing.Function.SineInOut);// Or use the Function enum.
    ///     
    ///     // Or apply it to whatever the current state happens to be.
    ///     CustomFade.Apply(_Animancer, Easing.Sine.InOut);
    ///     
    ///     // Anything else you play after that will automatically cancel the custom fade.
    /// }
    /// </code></example>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/CustomFade
    /// 
    public abstract partial class CustomFade : Key, IUpdatable
    {
        /************************************************************************************************************************/

        private float _Time;
        private float _FadeSpeed;
        private NodeWeight _Target;
        private AnimancerLayer _Layer;
        private int _CommandCount;

        private readonly List<NodeWeight> FadeOutNodes = new List<NodeWeight>();

        /************************************************************************************************************************/

        private readonly struct NodeWeight
        {
            public readonly AnimancerNode Node;
            public readonly float StartingWeight;

            public NodeWeight(AnimancerNode node)
            {
                Node = node;
                StartingWeight = node.Weight;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gathers the current details of the <see cref="AnimancerNode.Root"/> and register this
        /// <see cref="CustomFade"/> to be updated by it so that it can replace the regular fade behaviour.
        /// </summary>
        protected void Apply(AnimancerState state)
        {
            AnimancerUtilities.Assert(state.Parent != null, "Node is not connected to a layer.");

            Apply((AnimancerNode)state);

            var parent = state.Parent;
            for (int i = parent.ChildCount - 1; i >= 0; i--)
            {
                var other = parent.GetChild(i);
                if (other != state && other.FadeSpeed != 0)
                {
                    other.FadeSpeed = 0;
                    FadeOutNodes.Add(new NodeWeight(other));
                }
            }
        }

        /// <summary>
        /// Gathers the current details of the <see cref="AnimancerNode.Root"/> and register this
        /// <see cref="CustomFade"/> to be updated by it so that it can replace the regular fade behaviour.
        /// </summary>
        protected void Apply(AnimancerNode node)
        {
#if UNITY_ASSERTIONS
            AnimancerUtilities.Assert(node != null, "Node is null.");
            AnimancerUtilities.Assert(node.IsValid, "Node is not valid.");
            AnimancerUtilities.Assert(node.FadeSpeed != 0, $"Node is not fading ({nameof(node.FadeSpeed)} is 0).");

            var animancer = node.Root;
            AnimancerUtilities.Assert(animancer != null, $"{nameof(node)}.{nameof(node.Root)} is null.");

            if (OptionalWarning.CustomFadeBounds.IsEnabled())
            {
                if (CalculateWeight(0) != 0)
                    OptionalWarning.CustomFadeBounds.Log("CalculateWeight(0) != 0.", animancer.Component);
                if (CalculateWeight(1) != 1)
                    OptionalWarning.CustomFadeBounds.Log("CalculateWeight(1) != 1.", animancer.Component);
            }
#endif

            _Time = 0;
            _Target = new NodeWeight(node);
            _FadeSpeed = node.FadeSpeed;
            _Layer = node.Layer;
            _CommandCount = _Layer.CommandCount;

            node.FadeSpeed = 0;

            FadeOutNodes.Clear();

            node.Root.RequirePreUpdate(this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the desired weight for the target state at the specified `progress` (ranging from 0 to 1).
        /// </summary>
        /// <remarks>
        /// This method should return 0 when the `progress` is 0 and 1 when the `progress` is 1. It can do anything you
        /// want with other values, but violating that guideline will trigger
        /// <see cref="OptionalWarning.CustomFadeBounds"/>.
        /// </remarks>
        protected abstract float CalculateWeight(float progress);

        /// <summary>Called when this fade is cancelled (or ends).</summary>
        /// <remarks>Can be used to return it to an <see cref="ObjectPool"/>.</remarks>
        protected abstract void Release();

        /************************************************************************************************************************/

        void IUpdatable.Update()
        {
            // Stop fading if the state was destroyed or something else was played.
            if (!_Target.Node.IsValid() ||
                _Layer != _Target.Node.Layer ||
                _CommandCount != _Layer.CommandCount)
            {
                FadeOutNodes.Clear();
                _Layer.Root.CancelPreUpdate(this);
                Release();
                return;
            }

            _Time += AnimancerPlayable.DeltaTime * _Layer.Speed * _FadeSpeed;

            if (_Time < 1)// Fade.
            {
                var weight = CalculateWeight(_Time);

                _Target.Node.SetWeight(Mathf.LerpUnclamped(_Target.StartingWeight, _Target.Node.TargetWeight, weight));
                _Target.Node.ApplyWeight();

                weight = 1 - weight;
                for (int i = FadeOutNodes.Count - 1; i >= 0; i--)
                {
                    var node = FadeOutNodes[i];
                    node.Node.SetWeight(node.StartingWeight * weight);
                    node.Node.ApplyWeight();
                }
            }
            else// End.
            {
                _Time = 1;
                ForceFinishFade(_Target.Node);

                for (int i = FadeOutNodes.Count - 1; i >= 0; i--)
                    ForceFinishFade(FadeOutNodes[i].Node);

                FadeOutNodes.Clear();
                _Layer.Root.CancelPreUpdate(this);
                Release();
            }
        }

        /************************************************************************************************************************/

        private static void ForceFinishFade(AnimancerNode node)
        {
            var weight = node.TargetWeight;
            node.SetWeight(weight);
            node.ApplyWeight();
            if (weight == 0)
                node.Stop();
        }

        /************************************************************************************************************************/
    }
}
