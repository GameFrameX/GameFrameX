// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float2ControllerTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 2", order = Strings.AssetMenuOrder + 7)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float2ControllerTransitionAsset))]
    public class Float2ControllerTransitionAsset : AnimancerTransitionAsset<Float2ControllerTransition>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<Float2ControllerTransitionAsset, Float2ControllerTransition, Float2ControllerState>,
            Float2ControllerState.ITransition
        { }
    }

    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float2ControllerTransition
    [Serializable]
    public class Float2ControllerTransition : ControllerTransition<Float2ControllerState>,
        Float2ControllerState.ITransition, ICopyable<Float2ControllerTransition>
    {
        /************************************************************************************************************************/

        [SerializeField]
        private string _ParameterNameX;

        /// <summary>[<see cref="SerializeField"/>] The name that will be used to access <see cref="ParameterX"/>.</summary>
        public ref string ParameterNameX => ref _ParameterNameX;

        /************************************************************************************************************************/

        [SerializeField]
        private string _ParameterNameY;

        /// <summary>[<see cref="SerializeField"/>] The name that will be used to access <see cref="ParameterY"/>.</summary>
        public ref string ParameterNameY => ref _ParameterNameY;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="Float2ControllerTransition"/>.</summary>
        public Float2ControllerTransition() { }

        /// <summary>Creates a new <see cref="Float2ControllerTransition"/> with the specified Animator Controller and parameters.</summary>
        public Float2ControllerTransition(RuntimeAnimatorController controller, string parameterNameX, string parameterNameY)
        {
            Controller = controller;
            _ParameterNameX = parameterNameX;
            _ParameterNameY = parameterNameY;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Float2ControllerState CreateState()
            => State = new Float2ControllerState(Controller, _ParameterNameX, _ParameterNameY, ActionsOnStop);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(Float2ControllerTransition copyFrom)
        {
            CopyFrom((ControllerTransition<Float2ControllerState>)copyFrom);

            if (copyFrom == null)
            {
                _ParameterNameX = default;
                _ParameterNameY = default;
                return;
            }

            _ParameterNameX = copyFrom._ParameterNameX;
            _ParameterNameY = copyFrom._ParameterNameY;
        }

        /************************************************************************************************************************/
        #region Drawer
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        [UnityEditor.CustomPropertyDrawer(typeof(Float2ControllerTransition), true)]
        public class Drawer : ControllerTransition.Drawer
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Drawer"/> and sets the
            /// <see cref="ControllerTransition.Drawer.Parameters"/>.
            /// </summary>
            public Drawer() : base(nameof(_ParameterNameX), nameof(_ParameterNameY)) { }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}
