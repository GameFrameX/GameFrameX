using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BestHTTP.Examples
{
    public class Link : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public string url;
        public Texture2D linkSelectCursor;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            openWindow(this.url);
#else
            Application.OpenURL(this.url);
#endif
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Cursor.SetCursor(this.linkSelectCursor, Vector2.zero, CursorMode.Auto);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void openWindow(string url);
#endif
    }
}