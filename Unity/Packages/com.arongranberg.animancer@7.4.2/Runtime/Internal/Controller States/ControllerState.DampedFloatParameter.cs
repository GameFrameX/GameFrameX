// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/ControllerState
    partial class ControllerState
    {
        /************************************************************************************************************************/

        /// <summary>
        /// A wrapper for <see cref="Mathf.SmoothDamp(float, float, ref float, float, float, float)"/> to control float
        /// parameters in <see cref="ControllerState"/>s similar to
        /// <see cref="Animator.SetFloat(int, float, float, float)"/>.
        /// </summary>
        public class DampedFloatParameter
        {
            /************************************************************************************************************************/

            /// <summary>The name of this parameter.</summary>
            public ParameterID parameter;

            /// <summary>The amount of time allowed to smooth out a value change.</summary>
            public float smoothTime;

            /// <summary>The last value the parameter was set to.</summary>
            public float currentValue;

            /// <summary>The value that the parameter is moving towards.</summary>
            public float targetValue;

            /// <summary>The maximum speed that the current value can move towards the target.</summary>
            public float maxSpeed;

            /// <summary>The speed at which the value is currently moving.</summary>
            public float velocity;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="DampedFloatParameter"/>.</summary>
            public DampedFloatParameter(
                ParameterID parameter,
                float smoothTime,
                float defaultValue = 0,
                float maxSpeed = float.PositiveInfinity)
            {
                this.parameter = parameter;
                this.smoothTime = smoothTime;
                currentValue = targetValue = defaultValue;
                this.maxSpeed = maxSpeed;
            }

            /************************************************************************************************************************/

            /// <summary>Updates the target parameter.</summary>
            public void Apply(ControllerState controller)
                => Apply(controller, UnityEngine.Time.deltaTime);

            /// <summary>Updates the target parameter.</summary>
            public void Apply(ControllerState controller, float deltaTime)
            {
                currentValue = Mathf.SmoothDamp(currentValue, targetValue, ref velocity, smoothTime, maxSpeed, deltaTime);
                controller.SetFloat(parameter, currentValue);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

