using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.Helpers.SelectorUI
{
    public sealed class ExampleListItem : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Text _text;
#pragma warning restore

        public SampleSelectorUI ParentUI { get; private set; }

        public SampleBase ExamplePrefab { get; private set; }

        public void Setup(SampleSelectorUI parentUI, SampleBase prefab)
        {
            this.ParentUI = parentUI;
            this.ExamplePrefab = prefab;

            this._text.text = prefab.DisplayName;
        }

        public void OnButton()
        {
            this.ParentUI.SelectSample(this);
        }
    }
}