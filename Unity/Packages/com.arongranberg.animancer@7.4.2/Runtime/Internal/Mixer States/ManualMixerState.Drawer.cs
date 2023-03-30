// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/ManualMixerState
    partial class ManualMixerState
    {
        /************************************************************************************************************************/

        /// <summary>The number of parameters being managed by this state.</summary>
        protected virtual int ParameterCount => 0;

        /// <summary>Returns the name of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't manage any parameters.</exception>
        protected virtual string GetParameterName(int index)
            => throw new NotSupportedException();

        /// <summary>Returns the type of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't manage any parameters.</exception>
        protected virtual UnityEngine.AnimatorControllerParameterType GetParameterType(int index)
            => throw new NotSupportedException();

        /// <summary>Returns the value of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't manage any parameters.</exception>
        protected virtual object GetParameterValue(int index)
            => throw new NotSupportedException();

        /// <summary>Sets the value of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't manage any parameters.</exception>
        protected virtual void SetParameterValue(int index, object value)
            => throw new NotSupportedException();

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Returns a <see cref="Drawer{T}"/> for this state.</summary>
        protected internal override Editor.IAnimancerNodeDrawer CreateDrawer()
            => new Drawer<ManualMixerState>(this);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public class Drawer<T> : Editor.ParametizedAnimancerStateDrawer<T> where T : ManualMixerState
        {
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Drawer{T}"/> to manage the Inspector GUI for the `state`.</summary>
            public Drawer(T state)
                : base(state)
            { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override int ParameterCount
                => Target.ParameterCount;

            /// <inheritdoc/>
            public override string GetParameterName(int index)
                => Target.GetParameterName(index);

            /// <inheritdoc/>
            public override UnityEngine.AnimatorControllerParameterType GetParameterType(int index)
                => Target.GetParameterType(index);

            /// <inheritdoc/>
            public override object GetParameterValue(int index)
                => Target.GetParameterValue(index);

            /// <inheritdoc/>
            public override void SetParameterValue(int index, object value)
                => Target.SetParameterValue(index, value);

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

