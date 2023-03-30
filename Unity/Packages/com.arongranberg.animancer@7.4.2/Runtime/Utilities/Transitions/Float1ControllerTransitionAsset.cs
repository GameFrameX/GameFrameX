// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float1ControllerTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 1", order = Strings.AssetMenuOrder + 6)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float1ControllerTransitionAsset))]
    public class Float1ControllerTransitionAsset : AnimancerTransitionAsset<Float1ControllerTransition>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<Float1ControllerTransitionAsset, Float1ControllerTransition, Float1ControllerState>,
            Float1ControllerState.ITransition
        { }
    }

    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float1ControllerTransition
    [Serializable]
    public class Float1ControllerTransition : ControllerTransition<Float1ControllerState>,
        Float1ControllerState.ITransition, ICopyable<Float1ControllerTransition>
    {
        /************************************************************************************************************************/

        [SerializeField]
        private string _ParameterName;

        /// <summary>[<see cref="SerializeField"/>] The name that will be used to access <see cref="Parameter"/>.</summary>
        public ref string ParameterName => ref _ParameterName;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="Float1ControllerTransition"/>.</summary>
        public Float1ControllerTransition() { }

        /// <summary>Creates a new <see cref="Float1ControllerTransition"/> with the specified Animator Controller and parameter.</summary>
        public Float1ControllerTransition(RuntimeAnimatorController controller, string parameterName)
        {
            Controller = controller;
            _ParameterName = parameterName;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Float1ControllerState CreateState()
            => State = new Float1ControllerState(Controller, _ParameterName, ActionsOnStop);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(Float1ControllerTransition copyFrom)
        {
            CopyFrom((ControllerTransition<Float1ControllerState>)copyFrom);

            if (copyFrom == null)
            {
                _ParameterName = default;
                return;
            }

            _ParameterName = copyFrom._ParameterName;
        }

        /************************************************************************************************************************/
        #region Drawer
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        [UnityEditor.CustomPropertyDrawer(typeof(Float1ControllerTransition), true)]
        public class Drawer : ControllerTransition.Drawer
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Drawer"/> and sets the
            /// <see cref="ControllerTransition.Drawer.Parameters"/>.
            /// </summary>
            public Drawer() : base(nameof(_ParameterName)) { }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}
