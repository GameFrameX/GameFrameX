using BestHTTP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.Helpers.Components
{
    public class Cookies : MonoBehaviour
    {
#pragma warning disable 0649, 0169
        [SerializeField]
        private Text _count;

        [SerializeField]
        private Text _size;

        [SerializeField]
        private Button _clear;
#pragma warning restore

        private void Start()
        {
            PluginEventHelper.OnEvent += OnPluginEvent;
            UpdateLabels();
        }

        private void OnDestroy()
        {
            PluginEventHelper.OnEvent -= OnPluginEvent;
        }

        private void OnPluginEvent(PluginEventInfo @event)
        {
#if !BESTHTTP_DISABLE_COOKIES
            if (@event.Event == PluginEvents.SaveCookieLibrary)
                UpdateLabels();
#endif
        }

        private void UpdateLabels()
        {
#if !BESTHTTP_DISABLE_COOKIES
            var cookies = BestHTTP.Cookies.CookieJar.GetAll();
            var size = cookies.Sum(c => c.GuessSize());

            this._count.text = cookies.Count.ToString("N0");
            this._size.text = size.ToString("N0");
#else
            this._count.text = "0";
            this._size.text = "0";
#endif
        }

        public void OnClearButtonClicked()
        {
#if !BESTHTTP_DISABLE_COOKIES
            BestHTTP.Cookies.CookieJar.Clear();
#endif
        }
    }
}
