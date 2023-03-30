// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Events
{
    /// <summary>Uses Animancer Events to play a sound randomly selected from an array.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/events/footsteps">Footstep Events</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Events/FootstepEvents
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Footstep Events - Footstep Events")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Events) + "/" + nameof(FootstepEvents))]
    public sealed class FootstepEvents : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private ClipTransition _Walk;
        [SerializeField] private AudioClip[] _Sounds;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Animancer.Play(_Walk);
        }

        /************************************************************************************************************************/

        // Called by Animancer Events.
        public void PlaySound(AudioSource source)
        {
            source.clip = _Sounds[Random.Range(0, _Sounds.Length)];
            source.Play();
        }

        /************************************************************************************************************************/
    }
}
