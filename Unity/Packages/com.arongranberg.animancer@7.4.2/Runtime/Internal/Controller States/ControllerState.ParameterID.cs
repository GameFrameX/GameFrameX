// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/ControllerState
    partial class ControllerState
    {
        /************************************************************************************************************************/

        /// <summary>A wrapper for the name and hash of an <see cref="AnimatorControllerParameter"/>.</summary>
        public readonly struct ParameterID
        {
            /************************************************************************************************************************/

            /// <summary>The name of this parameter.</summary>
            public readonly string Name;

            /// <summary>The name hash of this parameter.</summary>
            public readonly int Hash;

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="ParameterID"/> with the specified <see cref="Name"/> and uses
            /// <see cref="Animator.StringToHash"/> to calculate the <see cref="Hash"/>.
            /// </summary>
            public ParameterID(string name)
            {
                Name = name;
                Hash = Animator.StringToHash(name);
            }

            /// <summary>
            /// Creates a new <see cref="ParameterID"/> with the specified <see cref="Hash"/> and leaves the
            /// <see cref="Name"/> null.
            /// </summary>
            public ParameterID(int hash)
            {
                Name = null;
                Hash = hash;
            }

            /// <summary>Creates a new <see cref="ParameterID"/> with the specified <see cref="Name"/> and <see cref="Hash"/>.</summary>
            /// <remarks>This constructor does not verify that the `hash` actually corresponds to the `name`.</remarks>
            public ParameterID(string name, int hash)
            {
                Name = name;
                Hash = hash;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="ParameterID"/> with the specified <see cref="Name"/> and uses
            /// <see cref="Animator.StringToHash"/> to calculate the <see cref="Hash"/>.
            /// </summary>
            public static implicit operator ParameterID(string name) => new ParameterID(name);

            /// <summary>
            /// Creates a new <see cref="ParameterID"/> with the specified <see cref="Hash"/> and leaves the
            /// <see cref="Name"/> null.
            /// </summary>
            public static implicit operator ParameterID(int hash) => new ParameterID(hash);

            /************************************************************************************************************************/

            /// <summary>Returns the <see cref="Hash"/>.</summary>
            public static implicit operator int(ParameterID parameter) => parameter.Hash;

            /************************************************************************************************************************/

            /// <summary>[Editor-Conditional]
            /// Throws if the `controller` doesn't have a parameter with the specified <see cref="Hash"/>
            /// and `type`.
            /// </summary>
            /// <exception cref="ArgumentException"/>
            [System.Diagnostics.Conditional(Strings.UnityEditor)]
            public void ValidateHasParameter(RuntimeAnimatorController controller, AnimatorControllerParameterType type)
            {
#if UNITY_EDITOR
                var parameterDetails = GetParameterDetails(controller);
                if (parameterDetails == null)
                    return;

                // Check that there is a parameter with the correct hash and type.

                if (!parameterDetails.TryGetValue(Hash, out var parameterType))
                {
                    throw new ArgumentException($"{controller} has no {type} parameter matching {this}");
                }

                if (type != parameterType)
                {
                    throw new ArgumentException($"{controller} has a parameter matching {this}, but it is not a {type}");
                }
#endif
            }

            /************************************************************************************************************************/

#if UNITY_EDITOR
            private static Dictionary<RuntimeAnimatorController, Dictionary<int, AnimatorControllerParameterType>>
                _ControllerToParameterHashAndType;

            /// <summary>[Editor-Only] Returns the hash mapped to the type of all parameters in the `controller`.</summary>
            /// <remarks>This doesn't work for if the `controller` was loaded from an Asset Bundle.</remarks>
            private static Dictionary<int, AnimatorControllerParameterType> GetParameterDetails(
                RuntimeAnimatorController controller)
            {
                Editor.AnimancerEditorUtilities.InitializeCleanDictionary(ref _ControllerToParameterHashAndType);

                if (_ControllerToParameterHashAndType.TryGetValue(controller, out var parameterDetails))
                    return parameterDetails;

                if (controller is AnimatorController editorController)
                {
                    var parameters = editorController.parameters;
                    var count = parameters.Length;

                    if (count != 0 || editorController.layers.Length != 0)
                    {
                        parameterDetails = new Dictionary<int, AnimatorControllerParameterType>();

                        for (int i = 0; i < count; i++)
                        {
                            var parameter = parameters[i];
                            parameterDetails.Add(parameter.nameHash, parameter.type);
                        }

                        _ControllerToParameterHashAndType.Add(controller, parameterDetails);
                        return parameterDetails;
                    }
                }

                _ControllerToParameterHashAndType.Add(controller, null);
                return null;
            }
#endif

            /************************************************************************************************************************/

            /// <summary>Returns a string containing the <see cref="Name"/> and <see cref="Hash"/>.</summary>
            public override string ToString()
            {
                return $"{nameof(ControllerState)}.{nameof(ParameterID)}" +
                    $"({nameof(Name)}: '{Name}'" +
                    $", {nameof(Hash)}: {Hash})";
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

