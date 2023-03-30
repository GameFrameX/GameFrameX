// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer.Examples
{
    /// <summary>Simple mouse controls for orbiting the camera around a focal point.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/examples/basics/scene-setup#orbit-controls">Orbit Controls</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples/OrbitControls
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Orbit Controls")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "." + nameof(Examples) + "/" + nameof(OrbitControls))]
    [ExecuteAlways]
    public sealed class OrbitControls : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private Vector3 _FocalPoint = new Vector3(0, 1, 0);
        [SerializeField] private Vector3 _Sensitivity = new Vector3(1, -0.75f, -0.1f);
        [SerializeField] private float _MinZoom = 0.5f;

        private float _Distance;

        /************************************************************************************************************************/

        private void Awake()
        {
            _Distance = Vector3.Distance(_FocalPoint, transform.position);

            transform.LookAt(_FocalPoint);
        }

        /************************************************************************************************************************/

        private void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                transform.LookAt(_FocalPoint);
                return;
            }
#endif

            if (ExampleInput.RightMouseHold)
            {
                var movement = ExampleInput.MousePositionDelta;
                if (!movement.Equals(default))
                {
                    var euler = transform.localEulerAngles;
                    euler.y += movement.x * _Sensitivity.x;
                    euler.x += movement.y * _Sensitivity.y;
                    if (euler.x > 180)
                        euler.x -= 360;
                    euler.x = Mathf.Clamp(euler.x, -80, 80);
                    transform.localEulerAngles = euler;
                }
            }

            // Scroll to zoom if the mouse is currently inside the game window.
            var zoom = ExampleInput.MouseScrollDelta.y * _Sensitivity.z;
            var mousePosition = ExampleInput.MousePosition;
            if (zoom != 0 &&
                mousePosition.x >= 0 && mousePosition.x <= Screen.width &&
                mousePosition.y >= 0 && mousePosition.y <= Screen.height)
            {
                _Distance *= 1 + zoom;
                if (_Distance < _MinZoom)
                    _Distance = _MinZoom;
            }

            // Always update position even with no input in case the target is moving.
            UpdatePosition();
        }

        /************************************************************************************************************************/

        private void UpdatePosition()
        {
            transform.position = _FocalPoint - transform.forward * _Distance;
        }

        /************************************************************************************************************************/

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 1, 0.5f, 1);
            Gizmos.DrawLine(transform.position, _FocalPoint);
        }

        /************************************************************************************************************************/
    }
}
