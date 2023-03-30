// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>An <see cref="AnimancerState"/> which plays an <see cref="AnimationClip"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ClipState
    /// 
    public class ClipState : AnimancerState
    {
        /************************************************************************************************************************/

        /// <summary>An <see cref="ITransition{TState}"/> that creates a <see cref="ClipState"/>.</summary>
        public interface ITransition : ITransition<ClipState> { }

        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimationClip"/> which this state plays.</summary>
        private AnimationClip _Clip;

        /// <summary>The <see cref="AnimationClip"/> which this state plays.</summary>
        public override AnimationClip Clip
        {
            get => _Clip;
            set
            {
                Validate.AssertNotLegacy(value);
                ChangeMainObject(ref _Clip, value);
            }
        }

        /// <summary>The <see cref="AnimationClip"/> which this state plays.</summary>
        public override Object MainObject
        {
            get => _Clip;
            set => Clip = (AnimationClip)value;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimationClip.length"/>.</summary>
        public override float Length => _Clip.length;

        /************************************************************************************************************************/

        /// <summary>The <see cref="Motion.isLooping"/>.</summary>
        public override bool IsLooping => _Clip.isLooping;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Vector3 AverageVelocity => _Clip.averageSpeed;

        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool ApplyAnimatorIK
        {
            get => _Playable.IsValid() && ((AnimationClipPlayable)_Playable).GetApplyPlayableIK();
            set
            {
                Validate.AssertPlayable(this);
                ((AnimationClipPlayable)_Playable).SetApplyPlayableIK(value);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool ApplyFootIK
        {
            get => _Playable.IsValid() && ((AnimationClipPlayable)_Playable).GetApplyFootIK();
            set
            {
                Validate.AssertPlayable(this);
                ((AnimationClipPlayable)_Playable).SetApplyFootIK(value);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ClipState"/> and sets its <see cref="Clip"/>.</summary>
        /// <exception cref="ArgumentNullException">The `clip` is null.</exception>
        public ClipState(AnimationClip clip)
        {
            Validate.AssertNotLegacy(clip);
            _Clip = clip;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimationClipPlayable"/> managed by this node.</summary>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = AnimationClipPlayable.Create(Root._Graph, _Clip);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            _Clip = null;
            base.Destroy();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(AnimancerPlayable root)
        {
            var clone = new ClipState(_Clip);
            clone.SetNewCloneRoot(root);
            ((ICopyable<AnimancerState>)clone).CopyFrom(this);
            return clone;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inspector
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Returns a <see cref="Drawer"/> for this state.</summary>
        protected internal override Editor.IAnimancerNodeDrawer CreateDrawer() => new Drawer(this);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public class Drawer : Editor.AnimancerStateDrawer<ClipState>
        {
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Drawer"/> to manage the Inspector GUI for the `state`.</summary>
            public Drawer(ClipState state) : base(state) { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void AddContextMenuFunctions(UnityEditor.GenericMenu menu)
            {
                menu.AddDisabledItem(new GUIContent(
                    $"{DetailsPrefix}Animation Type: {Editor.AnimationBindings.GetAnimationType(Target._Clip)}"));

                base.AddContextMenuFunctions(menu);

                Editor.AnimancerEditorUtilities.AddContextMenuIK(menu, Target);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}

