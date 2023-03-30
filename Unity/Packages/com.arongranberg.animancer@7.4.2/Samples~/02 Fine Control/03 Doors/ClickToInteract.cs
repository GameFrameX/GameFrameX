// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer.Examples.FineControl
{
    /// <summary>
    /// Attempts to interact with whatever <see cref="IInteractable"/> the cursor is pointing at when the user clicks
    /// the mouse.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fine-control/doors">Doors</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.FineControl/ClickToInteract
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Fine Control - Click To Interact")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(FineControl) + "/" + nameof(ClickToInteract))]
    public sealed class ClickToInteract : MonoBehaviour
    {
        /************************************************************************************************************************/

        private void Update()
        {
            if (!ExampleInput.LeftMouseUp)
                return;

            var ray = Camera.main.ScreenPointToRay(ExampleInput.MousePosition);

            if (Physics.Raycast(ray, out var raycastHit))
            {
                var interactable = raycastHit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                    interactable.Interact();
            }
        }

        /************************************************************************************************************************/
    }
}
