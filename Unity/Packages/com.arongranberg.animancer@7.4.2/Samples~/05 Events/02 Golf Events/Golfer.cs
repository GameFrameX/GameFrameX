// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using UnityEngine;

namespace Animancer.Examples.Events
{
    /// <summary>Manages a character with the ability to hit a golf ball.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/events/golf">Golf Events</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Events/Golfer
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Golf Events - Golfer")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Events) + "/" + nameof(Golfer))]
    public sealed class Golfer : MonoBehaviour
    {
        /************************************************************************************************************************/

        private const string HitEventName = "Hit";

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private ClipTransition _Ready;
        [SerializeField, EventNames(HitEventName)] private ClipTransition _Swing;
        [SerializeField] private Rigidbody _Ball;
        [SerializeField] private Vector3 _HitVelocity = new Vector3(0, 10, 10);
        [SerializeField, Meters] private float _BallReturnHeight = -10;

        private Vector3 _BallStartPosition;

        /************************************************************************************************************************/

        private void Awake()
        {
            _BallStartPosition = _Ball.position;
            _Ball.isKinematic = true;

            _Swing.Events.SetCallback(HitEventName, HitBall);
            _Swing.Events.OnEnd = EndSwing;

            _Animancer.Play(_Ready);
        }

        /************************************************************************************************************************/

        private void ResetBall()
        {
            _Ball.isKinematic = true;
            _Ball.position = _BallStartPosition;
        }

        /************************************************************************************************************************/

        private void Update()
        {
            if (_Ball.isKinematic)
            {
                if (ExampleInput.LeftMouseDown)
                {
                    _Animancer.Play(_Swing);
                }
            }
            else if (_Ball.position.y < _BallReturnHeight)
            {
                ResetBall();
            }
        }

        /************************************************************************************************************************/

        private void HitBall()
        {
            _Ball.isKinematic = false;
            _Ball.velocity = _HitVelocity;
        }

        /************************************************************************************************************************/

        private void EndSwing()
        {
            // Since the swing animation is ending early, we want it to calculate the fade duration to fade out over
            // the remainder of that animation instead of just using the value specified by the _Ready transition.
            var fadeDuration = AnimancerEvent.GetFadeOutDuration();
            _Animancer.Play(_Ready, fadeDuration);
        }

        /************************************************************************************************************************/
    }
}
