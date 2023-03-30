// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using Animancer.Units;
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>Uses player input to control a <see cref="Character"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/weapons">Weapons</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/WeaponsCharacterBrain
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Weapons - Weapons Character Brain")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(WeaponsCharacterBrain))]
    public sealed class WeaponsCharacterBrain : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private Character _Character;
        [SerializeField] private CharacterState _Move;
        [SerializeField] private CharacterState _Attack;
        [SerializeField, Seconds] private float _InputTimeOut = 0.5f;
        [SerializeField] private EquipState _Equip;
        [SerializeField] private Weapon[] _Weapons;

        private StateMachine<CharacterState>.InputBuffer _InputBuffer;

        /************************************************************************************************************************/

        private void Awake()
        {
            _InputBuffer = new StateMachine<CharacterState>.InputBuffer(_Character.StateMachine);
        }

        /************************************************************************************************************************/

        private void Update()
        {
            UpdateMovement();
            UpdateEquip();
            UpdateAction();

            _InputBuffer.Update();
        }

        /************************************************************************************************************************/

        private void UpdateMovement()// This method is identical to the one in MovingCharacterBrain.
        {
            var input = ExampleInput.WASD;
            if (input != default)
            {
                // Get the camera's forward and right vectors and flatten them onto the XZ plane.
                var camera = Camera.main.transform;

                var forward = camera.forward;
                forward.y = 0;
                forward.Normalize();

                var right = camera.right;
                right.y = 0;
                right.Normalize();

                // Build the movement vector by multiplying the input by those axes.
                _Character.Parameters.MovementDirection =
                   right * input.x +
                   forward * input.y;

                // Enter the locomotion state if we aren't already in it.
                _Character.StateMachine.TrySetState(_Move);
            }
            else
            {
                _Character.Parameters.MovementDirection = default;
                _Character.StateMachine.TrySetDefaultState();
            }

            // Indicate whether the character wants to run or not.
            _Character.Parameters.WantsToRun = ExampleInput.LeftShiftHold;
        }

        /************************************************************************************************************************/

        private void UpdateEquip()
        {
            if (ExampleInput.RightMouseDown)
            {
                var equippedWeaponIndex = Array.IndexOf(_Weapons, _Character.Equipment.Weapon);

                equippedWeaponIndex++;
                if (equippedWeaponIndex >= _Weapons.Length)
                    equippedWeaponIndex = 0;

                _Equip.NextWeapon = _Weapons[equippedWeaponIndex];
                _InputBuffer.Buffer(_Equip, _InputTimeOut);
            }
        }

        /************************************************************************************************************************/

        private void UpdateAction()
        {
            if (ExampleInput.LeftMouseDown)
            {
                _InputBuffer.Buffer(_Attack, _InputTimeOut);
            }
        }

        /************************************************************************************************************************/
    }
}
