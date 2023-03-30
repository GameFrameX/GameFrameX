// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerTransitionAsset_1
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(AnimancerTransitionAsset<ITransition>) + "_1")]
    public class AnimancerTransitionAsset<TTransition> : AnimancerTransitionAssetBase
        where TTransition : ITransition
    {
        /************************************************************************************************************************/

        [SerializeReference]
        private TTransition _Transition;

        /// <summary>[<see cref="SerializeReference"/>]
        /// The <see cref="ITransition"/> wrapped by this <see cref="ScriptableObject"/>.
        /// </summary>
        /// <remarks>
        /// WARNING: the <see cref="AnimancerTransition{TState}.State"/> holds the most recently played state, so
        /// if you are sharing this transition between multiple objects it will only remember one of them.
        /// Consider using an <see cref="AnimancerTransitionAssetBase.UnShared{TAsset}"/>.
        /// <para></para>
        /// You can use <see cref="AnimancerPlayable.StateDictionary.GetOrCreate(ITransition)"/> or
        /// <see cref="AnimancerLayer.GetOrCreateState(ITransition)"/> to get or create the state for a
        /// specific object.
        /// </remarks>
        public TTransition Transition
        {
            get
            {
                AssertTransition();
                return _Transition;
            }
            set => _Transition = value;
        }

        /// <summary>Returns the <see cref="ITransition"/> wrapped by this <see cref="ScriptableObject"/>.</summary>
        public override ITransition GetTransition()
        {
            AssertTransition();
            return _Transition;
        }

        /************************************************************************************************************************/

        /// <summary>Is the <see cref="Transition"/> assigned (i.e. not <c>null</c>)?</summary>
        public bool HasTransition => _Transition != null;

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Logs an error if the <see cref="Transition"/> is null.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        private void AssertTransition()
        {
            if (_Transition == null)
                Debug.LogError($"'{name}' {nameof(Transition)} is not assigned." +
                    $" {nameof(HasTransition)} can be used to check without triggering this error.", this);
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// Assigns a default <typeparamref name="TTransition"/> to the <see cref="Transition"/> field.
        /// </summary>
        protected virtual void Reset()
        {
            _Transition = Editor.TypeSelectionButton.CreateDefaultInstance<TTransition>();
        }
#endif

        /************************************************************************************************************************/
    }
}
