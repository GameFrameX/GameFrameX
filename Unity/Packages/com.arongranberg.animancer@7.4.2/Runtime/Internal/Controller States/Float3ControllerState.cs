// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Pro-Only] A <see cref="ControllerState"/> which manages three float parameters.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/animator-controllers">Animator Controllers</see>
    /// </remarks>
    /// <seealso cref="Float1ControllerState"/>
    /// <seealso cref="Float2ControllerState"/>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float3ControllerState
    /// 
    public class Float3ControllerState : ControllerState
    {
        /************************************************************************************************************************/

        /// <summary>An <see cref="ITransition{TState}"/> that creates a <see cref="Float3ControllerState"/>.</summary>
        public new interface ITransition : ITransition<Float3ControllerState> { }

        /************************************************************************************************************************/

        private ParameterID _ParameterXID;

        /// <summary>The identifier of the parameter which <see cref="ParameterX"/> will get and set.</summary>
        public ParameterID ParameterXID
        {
            get => _ParameterXID;
            set
            {
                _ParameterXID = value;
                _ParameterXID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

        /// <summary>
        /// Gets and sets a float parameter in the <see cref="ControllerState.Controller"/> using the
        /// <see cref="ParameterXID"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is NaN or Infinity.</exception>
        public float ParameterX
        {
            get => Playable.GetFloat(_ParameterXID.Hash);
            set
            {
                AssertParameterValue(value);
                Playable.SetFloat(_ParameterXID.Hash, value);
            }
        }

        /************************************************************************************************************************/

        private ParameterID _ParameterYID;

        /// <summary>The identifier of the parameter which <see cref="ParameterY"/> will get and set.</summary>
        public ParameterID ParameterYID
        {
            get => _ParameterYID;
            set
            {
                _ParameterYID = value;
                _ParameterYID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

        /// <summary>
        /// Gets and sets a float parameter in the <see cref="ControllerState.Controller"/> using the
        /// <see cref="ParameterYID"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is NaN or Infinity.</exception>
        public float ParameterY
        {
            get => Playable.GetFloat(_ParameterYID.Hash);
            set
            {
                AssertParameterValue(value);
                Playable.SetFloat(_ParameterYID.Hash, value);
            }
        }

        /************************************************************************************************************************/

        private ParameterID _ParameterZID;

        /// <summary>The identifier of the parameter which <see cref="ParameterZ"/> will get and set.</summary>
        public ParameterID ParameterZID
        {
            get => _ParameterZID;
            set
            {
                _ParameterZID = value;
                _ParameterZID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

        /// <summary>
        /// Gets and sets a float parameter in the <see cref="ControllerState.Controller"/> using the
        /// <see cref="ParameterZID"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is NaN or Infinity.</exception>
        public float ParameterZ
        {
            get => Playable.GetFloat(_ParameterZID.Hash);
            set
            {
                AssertParameterValue(value);
                Playable.SetFloat(_ParameterZID.Hash, value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets and sets <see cref="ParameterX"/>, <see cref="ParameterY"/>, and <see cref="ParameterZ"/>.
        /// </summary>
        public Vector3 Parameter
        {
            get => new Vector3(ParameterX, ParameterY, ParameterZ);
            set
            {
                ParameterX = value.x;
                ParameterY = value.y;
                ParameterZ = value.z;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="Float3ControllerState"/> to play the `controller`.</summary>
        public Float3ControllerState(RuntimeAnimatorController controller,
            ParameterID parameterX, ParameterID parameterY, ParameterID parameterZ, params ActionOnStop[] actionsOnStop)
            : base(controller, actionsOnStop)
        {
            _ParameterXID = parameterX;
            _ParameterXID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);

            _ParameterYID = parameterY;
            _ParameterYID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);

            _ParameterZID = parameterZ;
            _ParameterZID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
        }

        /// <summary>Creates a new <see cref="Float3ControllerState"/> to play the `controller`.</summary>
        public Float3ControllerState(RuntimeAnimatorController controller,
            ParameterID parameterX, ParameterID parameterY, ParameterID parameterZ)
            : this(controller, parameterX, parameterY, parameterZ, null)
        { }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int ParameterCount => 3;

        /// <inheritdoc/>
        public override int GetParameterHash(int index)
        {
            switch (index)
            {
                case 0: return _ParameterXID;
                case 1: return _ParameterYID;
                case 2: return _ParameterZID;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            };
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(AnimancerPlayable root)
        {
            var clone = new Float3ControllerState(Controller, _ParameterXID, _ParameterYID, _ParameterZID);
            clone.SetNewCloneRoot(root);
            ((ICopyable<ControllerState>)clone).CopyFrom(this);
            return clone;
        }

        /************************************************************************************************************************/
    }
}

