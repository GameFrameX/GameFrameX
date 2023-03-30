// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float3ControllerTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 3", order = Strings.AssetMenuOrder + 8)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float3ControllerTransitionAsset))]
    public class Float3ControllerTransitionAsset : AnimancerTransitionAsset<Float3ControllerTransition>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<Float3ControllerTransitionAsset, Float3ControllerTransition, Float3ControllerState>,
            Float3ControllerState.ITransition
        { }
    }

    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float3ControllerTransition
    [Serializable]
    public class Float3ControllerTransition : ControllerTransition<Float3ControllerState>,
        Float3ControllerState.ITransition, ICopyable<Float3ControllerTransition>
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

        [SerializeField]
        private string _ParameterNameZ;

        /// <summary>[<see cref="SerializeField"/>] The name that will be used to access <see cref="ParameterZ"/>.</summary>
        public ref string ParameterNameZ => ref _ParameterNameZ;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="Float3ControllerTransition"/>.</summary>
        public Float3ControllerTransition() { }

        /// <summary>Creates a new <see cref="Float3ControllerTransition"/> with the specified Animator Controller and parameters.</summary>
        public Float3ControllerTransition(RuntimeAnimatorController controller,
            string parameterNameX, string parameterNameY, string parameterNameZ)
        {
            Controller = controller;
            _ParameterNameX = parameterNameX;
            _ParameterNameY = parameterNameY;
            _ParameterNameZ = parameterNameZ;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Float3ControllerState CreateState()
            => State = new Float3ControllerState(Controller, _ParameterNameX, _ParameterNameY, _ParameterNameZ, ActionsOnStop);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(Float3ControllerTransition copyFrom)
        {
            CopyFrom((ControllerTransition<Float3ControllerState>)copyFrom);

            if (copyFrom == null)
            {
                _ParameterNameX = default;
                _ParameterNameY = default;
                _ParameterNameZ = default;
                return;
            }

            _ParameterNameX = copyFrom._ParameterNameX;
            _ParameterNameY = copyFrom._ParameterNameY;
            _ParameterNameZ = copyFrom._ParameterNameZ;
        }

        /************************************************************************************************************************/
        #region Drawer
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        [UnityEditor.CustomPropertyDrawer(typeof(Float3ControllerTransition), true)]
        public class Drawer : ControllerTransition.Drawer
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Drawer"/> and sets the
            /// <see cref="ControllerTransition.Drawer.Parameters"/>.
            /// </summary>
            public Drawer() : base(nameof(_ParameterNameX), nameof(_ParameterNameY), nameof(_ParameterNameZ)) { }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}
