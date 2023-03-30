using BestHTTP.Examples.Helpers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples
{
    public static class GUIHelper
    {
        // https://en.wikipedia.org/wiki/Binary_prefix
        private static string[] prefixes = new string[] { " B", " KiB", " MiB", " GiB", " TiB" };
        public static string GetBytesStr(double bytes, byte precision)
        {
            int prefixIdx = 0;
            while (bytes >= 1024)
            {
                bytes = bytes / 1024;
                prefixIdx++;
            }

            return bytes.ToString("F" + precision) + prefixes[prefixIdx];
        }

        public static void RemoveChildren(RectTransform transform, int maxChildCount)
        {
            while (transform.childCount > maxChildCount)
            {
                var child = transform.GetChild(0);
                child.SetParent(null);

                GameObject.Destroy(child.gameObject);
            }
        }

        public static TextListItem AddText(TextListItem prefab, RectTransform contentRoot, string text, int maxEntries, ScrollRect scrollRect)
        {
            if (contentRoot == null)
                return null;

            var listItem = GameObject.Instantiate<TextListItem>(prefab, contentRoot, false);
            listItem.SetText(text);

            GUIHelper.RemoveChildren(contentRoot, maxEntries);

            if (scrollRect != null && scrollRect.isActiveAndEnabled)
                scrollRect.StartCoroutine(ScrollToBottom(scrollRect));

            return listItem;
        }

        public static IEnumerator ScrollToBottom(ScrollRect scrollRect)
        {
            yield return null;

            if (scrollRect != null && scrollRect.isActiveAndEnabled)
                scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }
}