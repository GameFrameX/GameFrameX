// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>Manages the items equipped by a <see cref="Character"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/weapons">Weapons</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/Equipment
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Weapons - Equipment")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(Equipment))]
    public sealed class Equipment : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private Transform _WeaponHolder;
        [SerializeField] private Weapon _Weapon;

        /************************************************************************************************************************/

        public Weapon Weapon
        {
            get => _Weapon;
            set
            {
                DetachWeapon();
                _Weapon = value;
                AttachWeapon();
            }
        }

        /************************************************************************************************************************/

        private void Awake()
        {
            AttachWeapon();
        }

        /************************************************************************************************************************/

        private void AttachWeapon()
        {
            if (_Weapon == null)
                return;

            var transform = _Weapon.transform;
            transform.parent = _WeaponHolder;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            _Weapon.gameObject.SetActive(true);
        }

        /************************************************************************************************************************/

        private void DetachWeapon()
        {
            if (_Weapon == null)
                return;

            _Weapon.transform.parent = transform;
            _Weapon.gameObject.SetActive(false);
        }

        /************************************************************************************************************************/
    }
}
